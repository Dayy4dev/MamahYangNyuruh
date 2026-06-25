using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 20f;
    public int damage = 15;

    private bool hasHit = false;

    private void OnEnable()
    {
        hasHit = false;
    }

    private void ApplyHit(Collider other)
    {
        if (hasHit) return;
        hasHit = true;

        IDamageable d = other.GetComponent<IDamageable>();
        if (d != null)
        {
            d.TakeDamage(damage);
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Deactivate projectile; HostileAI will enqueue it back to the pool via its coroutine
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        ApplyHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ApplyHit(collision.collider);
    }
}
