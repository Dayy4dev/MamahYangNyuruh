using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttack : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private AudioSource audioSource; // Komponen pemutar suara Player

    [Header("Audio Tracks (Suara Ayunan / Tembakan)")]
    [SerializeField] private AudioClip[] swordAttackCycles; // Masukkan 3 clip suara ayunan pedang (Element 0, 1, 2)
    [SerializeField] private AudioClip handCannonSound;     // Suara tembakan meriam tembak
    [SerializeField] private AudioClip squeekHammerSound;   // Suara palu mainan melengking

    [Header("Audio Tracks (Suara Pasang / Equip)")]
    [SerializeField] private AudioClip genericMeleeEquipSound; // Suara cabut pedang / angkat palu
    [SerializeField] private AudioClip handCannonEquipSound;   // Suara kokang / jepretan HandCannon

    [Header("Ranged Setup ")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("UI Cooldown Setup")]
    [SerializeField] private UnityEngine.UI.Image cooldownUiImage; // Tarik objek CooldownIndicator ke sini via Inspector
    private ObjectPool<Bullet> bulletPool;


    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private WeaponData currentWeaponData;
    private Weapon activeWeapon;
    private int comboStep = 0; // Melacak index ayunan pedang (0 -> 1 -> 2)

    private int attackDamage;
    private float attackDuration;
    private float attackCooldown;
    private float knockbackForce;
    private float knockbackDuration;

    private float attackTimer;
    private float cooldownTimer;
    private bool isAttacking;
    private bool isArmed;
    private PlayerHealth playerHealth;


    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------


    private void Awake()
    {
        bulletPool = new ObjectPool<Bullet>(
            createFunc: CreateBullet,
            actionOnGet: OnBulletGet,
            actionOnRelease: OnBulletRelease,
            actionOnDestroy: OnBulletDestroy,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50
        );
    }
    void Start()
    {

        playerHealth = GetComponent<PlayerHealth>();
    }

    // Fungsi Callback untuk Pool
    private Bullet CreateBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab);
        bulletObj.SetActive(false);
        return bulletObj.GetComponent<Bullet>();
    }

    private void OnBulletGet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void OnBulletRelease(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnBulletDestroy(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }

    void Update()
    {
        // 1. FIX PAUSE & MENU: Jangan izinkan menyerang/menembak jika game sedang di-pause atau membuka UI
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            // Pastikan hitbox langsung mati jika player mendadak mati saat mengayunkan senjata
            if (isAttacking) EndAttack();
            return;
        }

        TickAttack();
        TickCooldown();
        HandleInput();
        UpdateCooldownUI();
    }


    public void EquipWeapon(WeaponData data, Weapon weaponComponent)
    {
        EndAttack();

        currentWeaponData = data;
        activeWeapon = weaponComponent;
        isArmed = data != null;
        comboStep = 0;

        if (isArmed)
        {
            attackDamage = data.damage;
            attackDuration = data.attackDuration;
            attackCooldown = data.attackCooldown;
            knockbackForce = data.knockbackForce;
            knockbackDuration = data.knockbackDuration;

            if (audioSource != null)
            {
                if (data.weaponName.Contains("Hand_Cannon") || data.weaponName.Contains("HandCannon"))
                {
                    if (handCannonEquipSound != null) audioSource.PlayOneShot(handCannonEquipSound);
                }
                else
                {
                    if (genericMeleeEquipSound != null) audioSource.PlayOneShot(genericMeleeEquipSound);
                }
            }
        }
    }

    public void SetWeaponHitbox(WeaponHitbox newHitbox)
    {
        this.weaponHitbox = newHitbox;
    }

    public void ApplyKnockback(GameObject target)
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir.y = 0f;

        if (target.TryGetComponent<IKnockbackable>(out IKnockbackable kb))
            kb.TakeKnockback(dir, knockbackForce, knockbackDuration);
        else if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void HandleInput()
    {
        // Jika klik kiri ditekan, kita cek statusnya di console
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[Input Cek] Klik kiri ditekan! Status -> isArmed: {isArmed}, Klik Kanan Ditahan: {Input.GetMouseButton(1)}, IsAttacking: {isAttacking}, CooldownTimer: {cooldownTimer:F2}");
        }

        if (!isArmed) return;

        // Sesuai logika Anda: Harus tahan klik kanan (Aim) BARU klik kiri (Serang)
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0))
        {
            if (!isAttacking && cooldownTimer <= 0f)
            {
                StartAttack();
            }
            else
            {
                Debug.Log($"[Input Cek] Gagal StartAttack karena -> IsAttacking: {isAttacking} ATAU CooldownTimer masih: {cooldownTimer:F2}");
            }
        }
    }

    private void TickAttack()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
            EndAttack();
    }

    private void TickCooldown()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void StartAttack()
    {
        Debug.Log($"[Cek 1] Tombol klik dideteksi. Senjata aktif: {(currentWeaponData != null ? currentWeaponData.weaponName : "Kosong")}");
        if (currentWeaponData == null) return;

        // FIX TAMBAHAN: Jika player mati, batalkan paksa serangan di sini sebelum masuk ke logika HandCannon
        if (playerHealth != null && playerHealth.IsDead)
        {
            isAttacking = false;
            return;
        }

        // 1. CEK REHAT/RELOAD UTAMA
        if (activeWeapon != null && !activeWeapon.CanAttack())
        {
            Debug.Log($"[PlayerAttack] Serangan dibatalkan karena {currentWeaponData.weaponName} sedang rehat/reload/cooldown.");
            isAttacking = false;
            return;
        }

        // 2. JIKA LOLOS CEK, SET TIMER DEFAULT
        isAttacking = true;
        attackTimer = attackDuration;
        cooldownTimer = attackCooldown;

        Debug.Log($"[Cek 2] Masuk ke penentuan tipe senjata. Nama: {currentWeaponData.weaponName}");

        // 3. LOGIKA KHUSUS HANDCANNON
        if (currentWeaponData.weaponName == "Hand_Cannon" || currentWeaponData.weaponName == "HandCannon")
        {
            if (audioSource != null && handCannonSound != null)
                audioSource.PlayOneShot(handCannonSound);

            FireRangedWeapon();

            HandCannon handCannon = activeWeapon as HandCannon;
            if (handCannon != null)
                handCannon.ConsumeBullet();

            cooldownTimer = currentWeaponData.fireRate;
        }
        // 4. LOGIKA UNTUK TOY HAMMER
        else if (currentWeaponData.weaponName == "SqueekHammer" || currentWeaponData.weaponName == "ToyHammer")
        {
            if (audioSource != null && squeekHammerSound != null)
                audioSource.PlayOneShot(squeekHammerSound);

            if (activeWeapon != null) activeWeapon.Attack();
            else TriggerMeleeHitbox();
        }
        // == PERBAIKAN DI SINI: LOGIKA UNTUK UNARMED ==
        else if (currentWeaponData.weaponName == "Unarmed" || currentWeaponData.weaponName == "TanganKosong")
        {
            Debug.Log("[Cek 3] Berhasil masuk ke blok khusus Unarmed!");
            if (activeWeapon != null)
            {
                Debug.Log("[Cek 4] activeWeapon ditemukan, memanggil activeWeapon.Attack()...");
                activeWeapon.Attack();
            }
            else
            {
                Debug.Log("[Cek 4] activeWeapon NULL! Memanggil TriggerMeleeHitbox() player...");
                TriggerMeleeHitbox();
            }
        }
        // 5. LOGIKA DEFAULT (BALLOON SWORD / SENJATA PEDANG LAINNYA)
        else
        {
            Debug.Log("[Cek 3] Masuk ke blok Default (Sword)");
            PlaySwordComboSound();
            if (activeWeapon != null) activeWeapon.Attack();
            else TriggerMeleeHitbox();
        }
    }

    private void PlaySwordComboSound()
    {
        if (swordAttackCycles == null || swordAttackCycles.Length == 0) return;

        if (audioSource != null && swordAttackCycles[comboStep] != null)
        {
            audioSource.PlayOneShot(swordAttackCycles[comboStep]);
        }

        comboStep = (comboStep + 1) % swordAttackCycles.Length;
    }

    private void TriggerMeleeHitbox()
    {
        if (weaponHitbox == null)
        {
            Debug.LogWarning("[PlayerAttack] WeaponHitbox belum terpasang untuk senjata Melee!");
            return;
        }
        weaponHitbox.Activate(attackDamage);
    }

    private void FireRangedWeapon()
    {
        // FIX TAMBAHAN: Peluru tidak akan di-spawn sama sekali jika player mati
        if (playerHealth != null && playerHealth.IsDead) return;

        if (bulletPrefab == null)
        {
            Debug.LogError("[PlayerAttack] Bullet Prefab belum dimasukkan di Inspector!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 1f;
        Quaternion spawnRot = transform.rotation;

        Bullet bullet = bulletPool.Get();
        bullet.transform.SetPositionAndRotation(spawnPos, spawnRot);
        bullet.SetPool(bulletPool);
        bullet.Setup(currentWeaponData.speed, currentWeaponData.damage);

        if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
        }

        Debug.Log($"[PlayerAttack] Menembakkan {currentWeaponData.weaponName}!");
    }

    private void EndAttack()
    {
        isAttacking = false;
        weaponHitbox?.Deactivate();
    }

    // Tambahkan ini di PlayerAttack.cs agar PlayerInventory bisa mengakses senjata saat ini
    public Weapon GetActiveWeapon()
    {
        return activeWeapon;
    }

    private void UpdateCooldownUI()
    {
        if (cooldownUiImage == null) return;

        // Jika tidak memegang senjata, sembunyikan UI
        if (!isArmed || activeWeapon == null)
        {
            cooldownUiImage.gameObject.SetActive(false);
            return;
        }

        // Ambil persentase cooldown dari senjata aktif
        float cooldownProgress = activeWeapon.GetCooldownPercentage();

        if (cooldownProgress > 0f)
        {
            // Aktifkan UI dan isi fillem-nya sesuai sisa waktu rehat
            cooldownUiImage.gameObject.SetActive(true);
            cooldownUiImage.fillAmount = cooldownProgress;
        }
        else
        {
            // Sembunyikan UI jika senjata siap digunakan kembali
            cooldownUiImage.gameObject.SetActive(false);
        }
    }
}