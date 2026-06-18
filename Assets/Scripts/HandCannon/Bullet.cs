using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damageAmount = 10;
    
    private float currentLifetime;
    private IObjectPool<Bullet> originPool;
    private bool hasHit = false;

    public void SetPool(IObjectPool<Bullet> pool)
    {
        originPool = pool;
    }

    private void OnEnable()
    {
        currentLifetime = lifetime;
        hasHit = false;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0)
        {
            ReleaseToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cek apakah objek yang tertabrak memiliki IDamageable interface
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !hasHit)
        {
            damageable.TakeDamage(damageAmount);
            hasHit = true;
            Debug.Log($"Bullet hit {other.gameObject.name} and dealt {damageAmount} damage!");
        }

        ReleaseToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Fallback untuk collider non-trigger
        var other = collision.collider;
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null && !hasHit)
        {
            damageable.TakeDamage(damageAmount);
            hasHit = true;
            Debug.Log($"Bullet collided with {other.gameObject.name} and dealt {damageAmount} damage!");
        }

        ReleaseToPool();
    }

    private void ReleaseToPool()
    {
        if (gameObject.activeSelf && originPool != null)
        {
            originPool.Release(this);
        }
    }
}