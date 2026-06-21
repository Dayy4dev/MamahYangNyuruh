using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[AddComponentMenu("Enemies/Melee NavMesh Enemy")]
public class MeleeEnemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    private Transform playerTransform;
    private EnemyKnockback enemyKnockback;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask playerLayerMask;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 8f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    [Header("Combat Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.2f;
    private bool isOnAttackCooldown;

    [Header("Detection Ranges")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private float engagementRange = 1.8f;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private void Awake()
    {
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        enemyKnockback = GetComponent<EnemyKnockback>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (enemyKnockback != null && enemyKnockback.IsKnockback) return;

        DetectPlayer();
        UpdateBehaviourState();
        UpdateAnimation();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"{gameObject.name} kena {amount} damage — HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} mati!");
        Destroy(gameObject, 0.1f);
    }

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

    private void UpdateBehaviourState()
    {
        if (playerTransform == null)
        {
            PerformPatrol();
            return;
        }

        if (!isPlayerVisible && !isPlayerInRange)
            PerformPatrol();
        else if (isPlayerVisible && !isPlayerInRange)
            PerformChase();
        else if (isPlayerVisible && isPlayerInRange)
            PerformAttack();
    }

    private bool IsNavReady() => navAgent != null && navAgent.enabled && navAgent.isOnNavMesh;

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 potentialPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(potentialPoint + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, terrainLayer))
        {
            currentPatrolPoint = hit.point;
            hasPatrolPoint = true;
        }
    }

    private void PerformPatrol()
    {
        if (!hasPatrolPoint) FindPatrolPoint();

        if (hasPatrolPoint && IsNavReady())
            navAgent.SetDestination(currentPatrolPoint);

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1.5f)
            hasPatrolPoint = false;
    }

    private void PerformChase()
    {
        if (playerTransform != null && IsNavReady())
        {
            navAgent.stoppingDistance = 0f;
            navAgent.SetDestination(playerTransform.position);
        }
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
            ExecuteMeleeStrike();
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private void ExecuteMeleeStrike()
    {
        Vector3 spherePos = transform.position + transform.forward * (engagementRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(spherePos, engagementRange, playerLayerMask);

        foreach (var c in hits)
        {
            if (c == null) continue;

            if (c.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(damage);
            else if (c.TryGetComponent<IDamageable>(out var d))
                d.TakeDamage(damage);
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCooldown = false;
    }

    private void UpdateAnimation()
    {
        if (animator == null || navAgent == null) return;

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