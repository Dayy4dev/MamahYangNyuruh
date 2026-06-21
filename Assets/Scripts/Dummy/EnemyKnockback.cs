using UnityEngine;
using UnityEngine.AI;

public class EnemyKnockback : MonoBehaviour, IKnockbackable
{
    private CharacterController cc;
    private Rigidbody rb;
    private NavMeshAgent navAgent;

    private Vector3 knockbackVelocity;
    private float knockbackTimer = 0f;

    public bool IsKnockback => knockbackTimer > 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();

        if (cc == null && rb == null)
            Debug.LogWarning($"[EnemyKnockback] {gameObject.name} tidak punya CharacterController atau Rigidbody!");
    }

    void Update()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;

            if (cc != null)
            {
                cc.Move(knockbackVelocity * Time.deltaTime);
                knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 10f);
            }

            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector3.zero;
                TryEnableNavAgent();
            }
        }
    }

    public void TakeKnockback(Vector3 direction, float force, float duration)
    {
        DisableNavAgent();

        if (cc != null)
        {
            knockbackVelocity = direction * force;
            knockbackTimer = duration;
        }
        else if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
            Invoke(nameof(TryEnableNavAgent), duration);
        }
    }

    private void DisableNavAgent()
    {
        if (navAgent == null) return;
        if (navAgent.enabled && navAgent.isOnNavMesh)
            navAgent.ResetPath();
        navAgent.enabled = false;
    }

    private void TryEnableNavAgent()
    {
        if (navAgent == null) return;
        // Cek apakah GameObject masih aktif dan ada di NavMesh sebelum enable
        if (gameObject.activeInHierarchy)
            navAgent.enabled = true;
    }
}