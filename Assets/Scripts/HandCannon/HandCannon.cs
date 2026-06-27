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
    }

    private void Update()
    {
        if (fireRateTimer > 0f)
            fireRateTimer -= Time.deltaTime;
    }

    public bool CanFire()
    {
        return !isReloading && currentBullet > 0 && fireRateTimer <= 0f;
    }

    public override bool CanAttack()
    {
        return CanFire();
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
        targetPos.y = firePoint.position.y; // Menyamakan tinggi agar peluru tidak menukik/menanjak

        // Menggunakan targetPos yang sudah dideklarasikan di atas
        Vector3 shootDirection = (targetPos - firePoint.position).normalized;

        bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

        ConsumeBullet();
    }

    private void OnDisable()
    {
        // Jika senjata dihancurkan/dibuang saat reload, amankan statenya
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        // RESET UTAMA: Jika dibuang, paksa senjata dalam kondisi siap pakai saat diambil lagi
        isReloading = false;
        remainingReloadTime = 0f;
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
            Debug.Log($"Reload dipause. Sisa waktu: {remainingReloadTime:F1}s");
        }
    }

    public override void OnWeaponActivate()
    {
        // Jika peluru kosong dan sedang tidak reload, paksa reload baru
        if (currentBullet <= 0 && !isReloading)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutine());
            return;
        }

        // Resume reload yang tertunda
        if (isReloading && reloadCoroutine == null && remainingReloadTime > 0)
        {
            reloadCoroutine = StartCoroutine(ReloadCoroutineWithRemainingTime(remainingReloadTime));
            Debug.Log($"Reload dilanjutkan. Sisa waktu: {remainingReloadTime:F1}s");
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        remainingReloadTime = reloadTime;
        Debug.Log("Reloading... " + reloadTime + "s");

        while (remainingReloadTime > 0f)
        {
            remainingReloadTime -= Time.deltaTime;
            yield return null; // Tunggu sampai frame berikutnya
        }

        CompleteReload();
    }

    private IEnumerator ReloadCoroutineWithRemainingTime(float remainingTime)
    {
        isReloading = true;
        remainingReloadTime = remainingTime;
        Debug.Log($"Reloading resumed... {remainingTime:F1}s");

        while (remainingReloadTime > 0f)
        {
            remainingReloadTime -= Time.deltaTime;
            yield return null;
        }

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

    public override float GetCooldownPercentage()
    {
        if (isReloading && reloadTime > 0f)
        {
            // Menghitung persentase sisa reload (1 = baru mulai reload, 0 = selesai)
            return remainingReloadTime / reloadTime;
        }
        return 0f;
    }
}