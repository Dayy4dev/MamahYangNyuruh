using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class HandCannon : Weapon
{
    [Header("Weapon Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("References")]
    [SerializeField] private WeaponData weaponData;

    // Internal state
    private int magazineSize;
    private float reloadTime;
    private int currentBullet;
    private bool isReloading;
    private Coroutine reloadCoroutine;
    private float remainingReloadTime;

    private IObjectPool<Bullet> bulletPool;

    private void Awake()
    {
        bulletPool = new ObjectPool<Bullet>(
            CreateBullet,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyPoolObject,
            true,
            20,
            50
        );
    }

    private void Start()
    {
        if (weaponData != null)
        {
            reloadTime = weaponData.reloadTime;
            magazineSize = weaponData.magazineSize;
        }

        // Diisi setelah weaponData dibaca, biar nilainya bener
        currentBullet = magazineSize;
    }

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

        if (currentBullet <= 0)
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
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            Debug.Log($"Reload paused. Sisa waktu: {remainingReloadTime:F1}s");
        }
    }

    public override void OnWeaponActivate()
    {
        if (isReloading && reloadCoroutine == null && remainingReloadTime > 0)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutineWithRemainingTime(remainingReloadTime));
            Debug.Log($"Reload resumed. Sisa waktu: {remainingReloadTime:F1}s");
        }
    }

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

    // OBJECT POOL CALLBACKS

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