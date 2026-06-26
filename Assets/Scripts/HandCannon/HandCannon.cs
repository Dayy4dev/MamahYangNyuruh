using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class HandCannon : Weapon
{
    [Header("Weapon Settings")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Laser Indicator")]
    [SerializeField] private LineRenderer laserLineRenderer;
    [SerializeField] private float laserDistance = 30f;

    [Header("References")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private PlayerMovement playerMovement;

    // Internal state
    private int magazineSize;
    private float reloadTime;
    private float fireRate;
    private int currentBullet;
    private bool isReloading;
    private Coroutine reloadCoroutine;
    private float remainingReloadTime;
    private float fireRateTimer;

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
            fireRate = weaponData.fireRate;
        }

        currentBullet = magazineSize;

        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (fireRateTimer > 0f)
            fireRateTimer -= Time.deltaTime;

        if (laserLineRenderer == null || firePoint == null || playerMovement == null) return;

        if (Input.GetMouseButton(1))
        {
            if (!laserLineRenderer.enabled) laserLineRenderer.enabled = true;

            laserLineRenderer.SetPosition(0, firePoint.position);

            Vector3 targetPos = playerMovement.GetMouseTargetPosition;

            targetPos.y = firePoint.position.y;

            laserLineRenderer.SetPosition(1, targetPos);
        }
        else
        {
            if (laserLineRenderer.enabled) laserLineRenderer.enabled = false;
        }
    }

    public bool CanFire()
    {
        return !isReloading && currentBullet > 0 && fireRateTimer <= 0f;
    }

    public void ConsumeBullet()
    {
        currentBullet--;

        fireRateTimer = fireRate;

        if (currentBullet <= 0)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
        }
    }

    public override void Attack()
    {
        if (!CanFire() || playerMovement == null) return;

        Bullet bullet = bulletPool.Get();
        bullet.transform.position = firePoint.position;

        Vector3 targetPos = playerMovement.GetMouseTargetPosition;
        targetPos.y = firePoint.position.y;

        Vector3 shootDirection = (targetPos - firePoint.position).normalized;

        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        ConsumeBullet();
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

        if (laserLineRenderer != null) laserLineRenderer.enabled = false;
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

    //pooled object

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