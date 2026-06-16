using UnityEngine;
using UnityEngine.Pool;

public class HandCannon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerAim playerAim;

    [Header("Pool Configuration")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 50;

    private IObjectPool<Bullet> bulletPool;

    private void Awake()
    {
        bulletPool = new ObjectPool<Bullet>(
            CreateBullet,         // 1. Callback saat membuat objek baru jika pool kosong
            OnTakeFromPool,       // 2. Callback saat peluru diambil dari pool
            OnReturnedToPool,     // 3. Callback saat peluru dikembalikan ke pool
            OnDestroyPoolObject,  // 4. Callback jika peluru dihancurkan karena melebihi kapasitas maksimal
            true,                 // Collection check (mencegah error jika rilis objek yang sama dua kali)
            defaultCapacity,
            maxPoolSize
        );
    }

    // private void Update()
    // {
    //     if (Input.GetButtonDown("Fire1"))
    //     {
    //         Shoot();
    //     }
    // }
    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // Memaksa karakter berputar membidik kursor dlu
            if (playerAim != null)
            {
                playerAim.AimTowardsMouse();
            }

            Shoot();
        }
    }

    private void Shoot()
    {
        Bullet bullet = bulletPool.Get();

        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
    }

    // IMPLEMENTASI CALLBACK OBJECT POOL

    private Bullet CreateBullet()
    {
        Bullet bulletInstance = Instantiate(bulletPrefab);
        bulletInstance.SetPool(bulletPool);
        return bulletInstance;
    }

    private void OnTakeFromPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
}