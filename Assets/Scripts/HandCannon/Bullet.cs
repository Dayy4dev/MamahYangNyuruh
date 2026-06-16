using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifetime = 2f;
    
    private float currentLifetime;
    private IObjectPool<Bullet> originPool;

    public void SetPool(IObjectPool<Bullet> pool)
    {
        originPool = pool;
    }

    private void OnEnable()
    {
        currentLifetime = lifetime;
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