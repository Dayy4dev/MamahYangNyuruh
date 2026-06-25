using UnityEngine;

public class EnemyKnockback : MonoBehaviour, IKnockbackable
{
    private CharacterController cc;
    private Rigidbody rb;

    private Vector3 knockbackVelocity;
    private float knockbackTimer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (cc == null && rb == null)
            Debug.LogWarning($"[EnemyKnockback] {gameObject.name} tidak punya CharacterController atau Rigidbody!");
    }

    void Update()
    {
        if (knockbackTimer <= 0f || cc == null) return;

        cc.Move(knockbackVelocity * Time.deltaTime);
        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 10f);
        knockbackTimer -= Time.deltaTime;
    }

    // -------------------------------------------------------------------------
    // IKnockbackable
    // -------------------------------------------------------------------------

    public void TakeKnockback(Vector3 direction, float force, float duration)
    {
        if (cc != null)
        {
            knockbackVelocity = direction * force;
            knockbackTimer    = duration;
        }
        else if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
}