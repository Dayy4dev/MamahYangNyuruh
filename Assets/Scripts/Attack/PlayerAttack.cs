using UnityEngine;

[AddComponentMenu("Player/Player Attack")]
public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    [Tooltip("Jeda waktu maksimal (detik) antar hit agar combo tidak ter-reset")]
    [SerializeField] private float comboResetDuration = 1.0f;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource attackAudioSource;
    [Tooltip("Masukkan 3 sound untuk Sword (Hit 1, Hit 2, Hit 3) secara berurutan")]
    [SerializeField] private AudioClip[] swordSounds = new AudioClip[3];
    [Tooltip("Masukkan 3 sound untuk Hammer (Hit 1, Hit 2, Hit 3) secara berurutan")]
    [SerializeField] private AudioClip[] hammerSounds = new AudioClip[3];
    [Tooltip("Masukkan sound tembakan khusus untuk HandCannon")]
    [SerializeField] private AudioClip[] handCannonSound = new AudioClip[3];

    private int currentComboHit = 0;
    private float lastAttackTime = 0f;

    private WeaponData currentWeaponData;
    private Weapon activeWeaponComponent;
    private WeaponHitbox weaponHitbox;
    private int permanentDamageBuff = 0;

    void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
        
        // Auto-assign AudioSource jika lupa ditarik di Inspector
        if (attackAudioSource == null)
            attackAudioSource = GetComponent<AudioSource>();
    }

    public void EquipWeapon(WeaponData data, Weapon weaponComponent)
    {
        currentWeaponData = data;
        activeWeaponComponent = weaponComponent;
        ResetCombo();
    }

    public void SetWeaponHitbox(WeaponHitbox hitbox)
    {
        weaponHitbox = hitbox;
    }

    public void ApplyPermanentDamageBuff(int amount)
    {
        permanentDamageBuff += amount;
    }

    public Weapon GetActiveWeapon()
    {
        return activeWeaponComponent;
    }

    public void ResetCombo()
    {
        currentComboHit = 0;
    }

   void Update()
{
    // Jika game sedang dipause, jangan panggil serangan
    if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        return;

    if (Input.GetMouseButtonDown(0))
    {
        Weapon activeWeapon = GetActiveWeapon();

        // Semua senjata selain HandCannon harus aim dulu
        if (!(activeWeapon is HandCannon))
        {
            if (!Input.GetMouseButton(1))
            {
                Debug.Log("[PlayerAttack] Harus Aim (Mouse Kanan) terlebih dahulu!");
                return;
            }
        }
        else
        {
            // HandCannon tetap memakai sistemnya sendiri
            HandCannon handCannon = (HandCannon)activeWeapon;

            if (!handCannon.CanShoot())
            {
                Debug.Log("[PlayerAttack] Serangan dibatalkan karena player tidak menahan klik kanan!");
                return;
            }
        }

        ExecuteAttack();
    }
}

   private void ExecuteAttack()
    {
        if (playerHealth != null && playerHealth.IsDead) return;

        if (activeWeaponComponent != null && !activeWeaponComponent.CanAttack()) return;

        float timeSinceLastAttack = Time.time - lastAttackTime;
        if (timeSinceLastAttack > comboResetDuration)
        {
            currentComboHit = 1; 
        }
        else
        {
            int maxCombo = (currentWeaponData != null) ? currentWeaponData.maxComboCount : 3;
            currentComboHit = (currentComboHit % maxCombo) + 1;
        }

        lastAttackTime = Time.time;

        string weaponName = (currentWeaponData != null) ? currentWeaponData.weaponName : "Tangan Kosong";
        Debug.Log($"[Attack Melee] Mengayunkan {weaponName} | Combo Hit: {currentComboHit}");

        // === LOGIKA BARU: MEMUTAR SUARA SENJATA BERDASARKAN COMBO ===
        PlayAttackSound();

        if (activeWeaponComponent != null)
        {
            activeWeaponComponent.Attack(); 
        }

        if (weaponHitbox != null)
        {
            weaponHitbox.ActivateHitbox();
        }
    }
    private void PlayAttackSound()
    {
        if (attackAudioSource == null || currentWeaponData == null) return;

        string category = GetWeaponCategory(currentWeaponData);
        AudioClip clipToPlay = null;
        
        // Index array untuk combo melee (Sword / Hammer)
        int soundIndex = Mathf.Clamp(currentComboHit - 1, 0, 2);

        if (category == "Sword")
        {
            if (swordSounds != null && soundIndex < swordSounds.Length)
                clipToPlay = swordSounds[soundIndex];
        }
        else if (category == "Hammer")
        {
            if (hammerSounds != null && soundIndex < hammerSounds.Length)
                clipToPlay = hammerSounds[soundIndex];
        }
        else if (category == "Cannon") // <--- TAMBAHKAN LOGIKA INI UNTUK HANDCANNON
        {
            if (handCannonSound != null && soundIndex < handCannonSound.Length)
                clipToPlay = handCannonSound[soundIndex];
        }

        if (clipToPlay != null)
        {
            attackAudioSource.PlayOneShot(clipToPlay);
        }
    }
    public void CalculateHitEffects(out int finalDamage, out float stunDuration)
    {
        // --- PENGAMAN UTAMA: Jika data senjata null, langsung hitung Tangan Kosong + Buff ---
        if (currentWeaponData == null)
        {
            finalDamage = currentWeaponData.damage + (permanentDamageBuff / currentWeaponData.maxComboCount); 
            stunDuration = 0.3f;
            Debug.LogWarning("[PlayerAttack] currentWeaponData null! Menggunakan rumus Tangan Kosong + Buff.");
            return;
        }

        // Ambil kategori secara aman
        string category = GetWeaponCategory(currentWeaponData);

        // --- RUMUS MELEE UNTUK CANNON/HANDCANNON ---
        if (category == "Cannon")
        {
            finalDamage = currentWeaponData.damage + (permanentDamageBuff / currentWeaponData.maxComboCount*2);
            stunDuration = 0f; 
            return;
        }

        // --- RUMUS MELEE UNTUK SWORD (BALLOONSWORD) & HAMMER ---
        int baseTotal = currentWeaponData.damage + (permanentDamageBuff / currentWeaponData.maxComboCount);
        finalDamage = baseTotal;
        stunDuration = 0.3f;

        if (category == "Sword" && currentComboHit == 3)
        {
            finalDamage = Mathf.RoundToInt(baseTotal * 1.20f);
        }
        else if (category == "Hammer" && currentComboHit == 2)
        {
            finalDamage = Mathf.RoundToInt(baseTotal * 1.25f);
            stunDuration = 0.8f;
        }
    }
    
    public int GetPermanentDamageBuff()
    {
        return permanentDamageBuff;
    }

    private string GetWeaponCategory(WeaponData data)
    {
        if (data == null || string.IsNullOrEmpty(data.weaponName)) return "Unknown";
        string nameLower = data.weaponName.ToLower();

        if (nameLower.Contains("sword") || nameLower.Contains("blade") || nameLower.Contains("calibur"))
            return "Sword";
        if (nameLower.Contains("hammer") || nameLower.Contains("mallet"))
            return "Hammer";
        if (nameLower.Contains("cannon") || nameLower.Contains("blaster"))
            return "Cannon";

        return "Unknown";
    }

    public WeaponData GetCurrentWeaponData()
    {
        return currentWeaponData; 
    }

    public void ApplyKnockback(GameObject target)
    {
        if (target == null || currentWeaponData == null) return;

        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 knockbackDirection = (target.transform.position - transform.position).normalized;
            knockbackDirection.y = 0; 

            float force = 5f;

            string category = GetWeaponCategory(currentWeaponData);
            if (category == "Hammer") force = 8f; 

            targetRb.AddForce(knockbackDirection * force, ForceMode.VelocityChange);
        }
    }
}