using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[AddComponentMenu("Enemies/Melee NavMesh Enemy")]
public class MeleeEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    private Transform playerTransform;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayer;     // Layer untuk lantai/ground panggung
    [SerializeField] private LayerMask playerLayerMask; // Layer untuk mendeteksi player saat memukul

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 8f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    [Header("Combat Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1.2f;
    private bool isOnAttackCooldown;

    [Header("Detection Ranges")]
    [SerializeField] private float visionRange = 10f;       // Jarak mulai mengejar
    [SerializeField] private float engagementRange = 1.8f;  // Jarak mulai memukul

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private void Awake()
    {
        // Cari player otomatis menggunakan Tag "Player"
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        DetectPlayer();
        UpdateBehaviourState();
        UpdateAnimation();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagementRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    private void DetectPlayer()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        if (playerTransform == null)
        {
            isPlayerVisible = false;
            isPlayerInRange = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        isPlayerVisible = distanceToPlayer <= visionRange;
        isPlayerInRange = distanceToPlayer <= engagementRange;
    }

    private void UpdateBehaviourState()
    {
        if (playerTransform == null)
        {
            PerformPatrol();
            return;
        }

        // State Machine penentu aksi musuh
        if (!isPlayerVisible && !isPlayerInRange)
        {
            PerformPatrol();
        }
        else if (isPlayerVisible && !isPlayerInRange)
        {
            PerformChase();
        }
        else if (isPlayerVisible && isPlayerInRange)
        {
            PerformAttack();
        }
    }

    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 potentialPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Menembakkan raycast ke bawah untuk mendeteksi tanah panggung
        if (Physics.Raycast(potentialPoint + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f, terrainLayer))
        {
            currentPatrolPoint = hit.point;
            hasPatrolPoint = true;
        }
    }

    private void PerformPatrol()
    {
        if (!hasPatrolPoint)
            FindPatrolPoint();

        if (hasPatrolPoint && navAgent != null && navAgent.isOnNavMesh)
            navAgent.SetDestination(currentPatrolPoint);

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1.5f)
            hasPatrolPoint = false;
    }

    private void PerformChase()
    {
        if (playerTransform != null && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.stoppingDistance = 0f;
            navAgent.SetDestination(playerTransform.position);
        }
    }

    private void PerformAttack()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(transform.position);
        }

        if (playerTransform != null)
        {
            Vector3 targetPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            transform.LookAt(targetPosition);
        }

        if (!isOnAttackCooldown)
        {
            ExecuteMeleeStrike();
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    private void ExecuteMeleeStrike()
    {
        // if (animator != null) animator.SetTrigger("Attack");

        Vector3 spherePos = transform.position + transform.forward * (engagementRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(spherePos, engagementRange, playerLayerMask);
        
        foreach (var c in hits)
        {
            if (c == null) continue;

            if (c.CompareTag("Player"))
            {
                Debug.Log("MeleeEnemy: BERHASIL MEMUKUL PLAYER -> " + c.gameObject.name);
            }

            if (c.TryGetComponent<PlayerHealth>(out var ph))
            {
                ph.TakeDamage(damage);
            }
            else if (c.TryGetComponent<IDamageable>(out var d))
            {
                d.TakeDamage(damage);
            }
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
        if (animator != null && navAgent != null)
        {
            Vector3 velocity = navAgent.velocity;
            
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);

            animator.SetFloat("Horizontal", localVelocity.x);
            animator.SetFloat("Vertical", localVelocity.z);
        }
    }
}