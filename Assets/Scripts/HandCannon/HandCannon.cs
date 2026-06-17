using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class HandCannon : Weapon
{

    [Header("Weapon Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;
    // [SerializeField] private PlayerAim playerAim;

    [Header("Pool Configuration")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxPoolSize = 50;

    // Internal state
    private int magazineSize;
    private float reloadTime;

    [Header("References")]
    [SerializeField]private WeaponData weaponData;
    private int currentBullet;
    private bool isReloading;
    private Coroutine reloadCoroutine;
    private float remainingReloadTime; // Menyimpan sisa waktu reload

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

    private void Start()
    {
        currentBullet = magazineSize;
        if (weaponData != null)
        {
            reloadTime = weaponData.reloadTime;
            magazineSize = weaponData.magazineSize;
        }
    }

    // private void Update()
    // {
    //     if (Input.GetButtonDown("Fire1"))
    //     {
    //         Shoot();
    //     }
    // }
    // private void Update()
    // {
    //     if (Input.GetButtonDown("Fire1"))
    //     {
    //         // Memaksa karakter berputar membidik kursor dlu
    //         if (playerAim != null)
    //         {
    //             playerAim.AimTowardsMouse();
    //         }

    //         Shoot();
    //     }
    // }

    public override void Attack()
    {

        if (isReloading)
        {
            Debug.Log("Reloading...");
            return;
        }

        Bullet bullet = bulletPool.Get();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        currentBullet--;
        Debug.Log($"Sisa peluru: {currentBullet}");

        if(currentBullet <= 0)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
    }

    public void ResetReloadState()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        isReloading = false;
        currentBullet = magazineSize;
        remainingReloadTime = 0f;
    }

    public override void OnWeaponDeactivate()
    {
        // Pause reload: hentikan coroutine tapi simpan progress
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            Debug.Log($"Reload paused. Sisa waktu: {remainingReloadTime:F1}s");
        }
    }

    public override void OnWeaponActivate()
    {
        // Resume reload dari sisa waktu
        if (isReloading && reloadCoroutine == null && remainingReloadTime > 0)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutineWithRemainingTime(remainingReloadTime));
            Debug.Log($"Reload resumed. Sisa waktu: {remainingReloadTime:F1}s");
        }
    }
    // public void Shoot()
    // {

    //     if (isReloading)
    //     {
    //         Debug.Log("Reloading...");
    //         return;
    //     }

    //     Bullet bullet = bulletPool.Get();
    //     bullet.transform.position = firePoint.position;
    //     bullet.transform.rotation = firePoint.rotation;

    //     currentBullet--;

    //     if(currentBullet <= 0)
    //     {
    //         StartCoroutine(ReloadCoroutine());
    //     }
    // }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        remainingReloadTime = reloadTime;
        Debug.Log("Reloading... " + reloadTime + "s");
        yield return new WaitForSeconds(remainingReloadTime);
        CompleteReload();
    }

    private IEnumerator ReloadCoroutineWithRemainingTime(float remainingTime)
    {
        isReloading = true;
        remainingReloadTime = remainingTime;
        Debug.Log($"Reloading resumed... {remainingTime:F1}s");
        yield return new WaitForSeconds(remainingReloadTime);
        CompleteReload();
    }

    private void CompleteReload()
    {
        currentBullet = magazineSize;
        isReloading = false;
        reloadCoroutine = null;
        remainingReloadTime = 0f;
        Debug.Log("Reloaded!");
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