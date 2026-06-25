using UnityEngine;

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

    [Header("Ranged Setup")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform firePoint;     

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private WeaponData currentWeaponData;
    private int comboStep = 0; // Melacak index ayunan pedang (0 -> 1 -> 2)

    private int   attackDamage;
    private float attackDuration;
    private float attackCooldown;
    private float knockbackForce;
    private float knockbackDuration;

    private float attackTimer;
    private float cooldownTimer;
    private bool  isAttacking;
    private bool  isArmed; 

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Update()
    {
        TickAttack();
        TickCooldown();
        HandleInput();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dipanggil PlayerInventory saat slot aktif berubah.
    /// </summary>
    public void EquipWeaponData(WeaponData data)
    {
        EndAttack(); 

        currentWeaponData = data;
        isArmed           = data != null;
        comboStep         = 0; // Setiap ganti senjata, urutan ayunan pedang di-reset ke awal

        if (isArmed)
        {
            attackDamage      = data.damage;
            attackDuration    = data.attackDuration;
            attackCooldown    = data.attackCooldown;
            knockbackForce    = data.knockbackForce;
            knockbackDuration = data.knockbackDuration;
            
            // --- LOGIKA SUARA EQUIP ---
            if (audioSource != null)
            {
                if (data.weaponName == "Hand_Cannon" || data.weaponName == "HandCannon")
                {
                    if (handCannonEquipSound != null) audioSource.PlayOneShot(handCannonEquipSound);
                }
                else
                {
                    if (genericMeleeEquipSound != null) audioSource.PlayOneShot(genericMeleeEquipSound);
                }
            }
            
            Debug.Log($"[PlayerAttack] Berhasil Sinkron Data & Suara Equip: {data.weaponName}");
        }
        else
        {
            Debug.Log("[PlayerAttack] Tidak memegang senjata (Unarmed)");
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
        if (!isArmed) return;

        // Tahan klik kanan untuk aim, klik kiri untuk attack
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0))
        {
            if (!isAttacking && cooldownTimer <= 0f)
                StartAttack();
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
        isAttacking   = true;
        attackTimer   = attackDuration;
        cooldownTimer = attackCooldown;

        // CEK JALUR SUARA DAN MEKANIK SENJATA
        if (currentWeaponData != null)
        {
            if (currentWeaponData.weaponName == "Hand_Cannon" || currentWeaponData.weaponName == "HandCannon")
            {
                // Putar suara HandCannon tembak
                if (audioSource != null && handCannonSound != null) 
                    audioSource.PlayOneShot(handCannonSound);

                FireRangedWeapon(); 
                Debug.Log("[PlayerAttack] HandCannon menembak!");
            }
            else if (currentWeaponData.weaponName == "SqueekHammer" || currentWeaponData.weaponName == "ToyHammer")
            {
                // Putar suara SqueekHammer mengayun
                if (audioSource != null && squeekHammerSound != null) 
                    audioSource.PlayOneShot(squeekHammerSound);

                TriggerMeleeHitbox();
            }
            else
            {
                // Senjata default dianggap pedang (memiliki 3 cycle suara ayunan bergantian)
                PlaySwordComboSound();
                TriggerMeleeHitbox();
            }
        }
    }

    private void PlaySwordComboSound()
    {
        if (swordAttackCycles == null || swordAttackCycles.Length == 0) return;

        // Putar clip sesuai langkah combo saat ini (0, 1, atau 2)
        if (audioSource != null && swordAttackCycles[comboStep] != null)
        {
            audioSource.PlayOneShot(swordAttackCycles[comboStep]);
        }

        // Geser ke langkah berikutnya, kalau sudah ke-3 balik lagi ke-0
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
        if (bulletPrefab == null)
        {
            Debug.LogError("[PlayerAttack] Bullet Prefab belum dimasukkan di Inspector!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 1f;
        Quaternion spawnRot = transform.rotation;

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnRot);

        if (bulletObj.TryGetComponent<Bullet>(out Bullet bullet))
        {
            if (bulletObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.linearVelocity = transform.forward * currentWeaponData.speed;
            }
        }
        
        Debug.Log($"[PlayerAttack] Menembakkan {currentWeaponData.weaponName}!");
    }

    private void EndAttack()
    {
        isAttacking = false;
        weaponHitbox?.Deactivate();
    }
}