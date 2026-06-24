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
    [SerializeField] private PlayerMovement playerMovement; // Tarik script PlayerMovement ke sini di Inspector

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

        // Pastikan laser mati di awal game jika tidak klik kanan
        if (laserLineRenderer != null)
        {
            laserLineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (laserLineRenderer == null || firePoint == null || playerMovement == null) return;

        // CEK INPUT: 1 adalah index untuk Klik Kanan
        if (Input.GetMouseButton(1))
        {
            // Aktifkan LineRenderer jika sedang menahan klik kanan
            if (!laserLineRenderer.enabled) laserLineRenderer.enabled = true;

            // Pangkal laser tetap di moncong
            laserLineRenderer.SetPosition(0, firePoint.position);

            // Ambil target horizontal dari posisi mouse
            Vector3 targetPos = playerMovement.GetMouseTargetPosition;

            // Kunci tinggi Y agar sejajar dengan moncong senjata, biar gak menukik ke lantai
            targetPos.y = firePoint.position.y;

            // Paksa ujung laser menunjuk ke target tersebut!
            laserLineRenderer.SetPosition(1, targetPos);
        }
        else
        {
            // Matikan LineRenderer jika klik kanan dilepas
            if (laserLineRenderer.enabled) laserLineRenderer.enabled = false;
        }
    }

    public override void Attack()
    {
        if (isReloading || playerMovement == null) return;

        Bullet bullet = bulletPool.Get();
        bullet.transform.position = firePoint.position;

        // Hitung arah dari moncong ke target mouse
        Vector3 targetPos = playerMovement.GetMouseTargetPosition;
        targetPos.y = firePoint.position.y; // Samakan tinggi Y

        Vector3 shootDirection = (targetPos - firePoint.position).normalized;

        // Paksa peluru menghadap ke arah tembakan yang benar
        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        currentBullet--;
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

        // Matikan laser saat senjata diganti/dimatikan
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