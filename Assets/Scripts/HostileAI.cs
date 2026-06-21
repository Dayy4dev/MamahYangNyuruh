using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1f;
    private bool isOnAttackCooldown;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float accuracyError = 0.5f;
    [SerializeField] private bool usePrediction = true;
    [SerializeField] private int projectilePoolSize = 10;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Detection Ranges")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    private bool isPlayerVisible;
    private bool isPlayerInRange;
    private readonly Queue<GameObject> projectilePool = new Queue<GameObject>();
    private Transform projectilePoolRoot;

    private EnemyKnockback enemyKnockback;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        enemyKnockback = GetComponent<EnemyKnockback>();

        InitializeProjectilePool();
    }

    private void Update()
    {
        // Skip AI saat kena knockback
        if (enemyKnockback != null && enemyKnockback.IsKnockback) return;

        DetectPlayer();
        UpdateBehaviourState();
    }

    private bool IsNavReady() => navAgent != null && navAgent.enabled && navAgent.isOnNavMesh;

    private void DetectPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (playerTransform == null)
        {
            isPlayerVisible = false;
            isPlayerInRange = false;
            return;
        }

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerVisible = dist <= visionRange;
        isPlayerInRange = dist <= engagementRange;
    }

    private void InitializeProjectilePool()
    {
        if (projectilePrefab == null) return;

        if (projectilePoolRoot == null)
            projectilePoolRoot = new GameObject("ProjectilePool - " + gameObject.name).transform;

        for (int i = 0; i < projectilePoolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectilePoolRoot);
            projectile.SetActive(false);
            projectilePool.Enqueue(projectile);
        }
    }

    private GameObject GetProjectileFromPool()
    {
        if (projectilePool.Count == 0)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectilePoolRoot);
            projectile.SetActive(false);
            projectilePool.Enqueue(projectile);
        }

        return projectilePool.Dequeue();
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || playerTransform == null) return;

        GameObject bullet = GetProjectileFromPool();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = Quaternion.identity;
        bullet.SetActive(true);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 targetPos = playerTransform.position + Vector3.up * 1f;

        if (usePrediction)
        {
            float distToTarget = Vector3.Distance(firePoint.position, targetPos);
            float travelTime = distToTarget / projectileSpeed;

            Vector3 playerVelocity = Vector3.zero;
            NavMeshAgent playerAgent = playerTransform.GetComponent<NavMeshAgent>();
            Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();

            if (playerAgent != null) playerVelocity = playerAgent.velocity;
            else if (playerRb != null) playerVelocity = playerRb.linearVelocity;

            targetPos += playerVelocity * travelTime;
        }

        Vector3 direction = (targetPos - firePoint.position).normalized;
        float travelEst = Vector3.Distance(firePoint.position, targetPos) / projectileSpeed;
        float gravOffset = 0.5f * Mathf.Abs(Physics.gravity.y) * travelEst * travelEst;
        direction = (direction + Vector3.up * (gravOffset / Vector3.Distance(firePoint.position, targetPos))).normalized;

        if (accuracyError > 0f)
        {
            direction += new Vector3(
                Random.Range(-accuracyError, accuracyError),
                Random.Range(-accuracyError, accuracyError),
                Random.Range(-accuracyError, accuracyError)
            ) * 0.05f;
            direction.Normalize();
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = direction * projectileSpeed;
        bullet.transform.forward = direction;

        StartCoroutine(ReturnProjectileToPool(bullet, projectileLifetime));
    }

    private IEnumerator ReturnProjectileToPool(GameObject projectile, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (projectile != null)
        {
            projectile.SetActive(false);
            projectile.transform.SetParent(projectilePoolRoot);
            projectilePool.Enqueue(projectile);
        }
    }

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 potentialPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(potentialPoint, -transform.up, 2f, terrainLayer))
        {
            currentPatrolPoint = potentialPoint;
            hasPatrolPoint = true;
        }
    }

    private void PerformPatrol()
    {
        if (!hasPatrolPoint) FindPatrolPoint();

        if (hasPatrolPoint && IsNavReady())
            navAgent.SetDestination(currentPatrolPoint);

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
            hasPatrolPoint = false;
    }

    private void PerformChase()
    {
        if (playerTransform != null && IsNavReady())
            navAgent.SetDestination(playerTransform.position);
    }

    private void PerformAttack()
    {
        if (IsNavReady())
            navAgent.SetDestination(transform.position);

        if (playerTransform != null)
        {
            Vector3 targetPos = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            transform.LookAt(targetPos);
        }

        if (!isOnAttackCooldown)
        {
            FireProjectile();
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCooldown = false;
    }

    private void UpdateBehaviourState()
    {
        if (!isPlayerVisible && !isPlayerInRange)
            PerformPatrol();
        else if (isPlayerVisible && !isPlayerInRange)
            PerformChase();
        else if (isPlayerVisible && isPlayerInRange)
            PerformAttack();

        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        if (animator == null || !IsNavReady()) return;

        Vector3 localVelocity = transform.InverseTransformDirection(navAgent.velocity);
        animator.SetFloat("Horizontal", localVelocity.x);
        animator.SetFloat("Vertical", localVelocity.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagementRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}