using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    private float speed;
    private float lifetime = 2f;
    private int damageAmount;

    private float currentLifetime;
    private IObjectPool<Bullet> originPool;
    private bool hasHit = false;

    [Header("References")]
    [SerializeField]private WeaponData weaponData;

        private void Start()
        {
            if (weaponData != null)
            {
                speed = weaponData.speed;
                damageAmount = weaponData.damage;            
            }
        }
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
            ReleaseToPool();

        }
        else if (!hasHit)
        {
            hasHit = true;
            Debug.Log($"Bullet hit {other.gameObject.name} but it is not damageable.");
            ReleaseToPool();
  
        }

    }

    private void ReleaseToPool()
    {
        if (gameObject.activeSelf && originPool != null)
        {
            originPool.Release(this);
        }
    }
}