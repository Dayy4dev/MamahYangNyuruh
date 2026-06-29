using UnityEngine;
using System;

[AddComponentMenu("Player/Player Attack")]
public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    [Tooltip("Jeda waktu maksimal (detik) antar hit agar combo tidak ter-reset")]
    [SerializeField] private float comboResetDuration = 1.0f;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private int currentComboHit = 0;
    private float lastAttackTime = 0f;

    private WeaponData currentWeaponData;
    private Weapon activeWeaponComponent;
    private WeaponHitbox weaponHitbox;
    private int permanentDamageBuff = 0;

    public event Action<string> OnWeaponEquipped;

    public event Action<int> OnAttackExecuted;

    void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    public void EquipWeapon(WeaponData data, Weapon weaponComponent)
    {
        currentWeaponData = data;
        activeWeaponComponent = weaponComponent;
        ResetCombo();

        string category = GetWeaponCategory(data);
        OnWeaponEquipped?.Invoke(category);
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

    public int GetCurrentComboHit()
    {
        return currentComboHit;
    }

    // Tambahkan fungsi Update ini di dalam PlayerAttack.cs
    void Update()
    {
        // Jika game sedang dipause, jangan panggil serangan
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        // Jika klik kiri (Tombol 0) ditekan, jalankan serangan
        if (Input.GetMouseButtonDown(0))
        {
            ExecuteAttack();
        }
    }

  private void ExecuteAttack()
{
    if (playerHealth != null && playerHealth.IsDead) return;

    // 1. Cek apakah senjata siap dipakai (tidak sedang reload/rehat)
    if (activeWeaponComponent != null && !activeWeaponComponent.CanAttack()) return;

    // 2. ATUR COMBO TERLEBIH DAHULU SEBELUM HITBOX SENJATA AKTIF
    float timeSinceLastAttack = Time.time - lastAttackTime;
    if (timeSinceLastAttack > comboResetDuration)
    {
        currentComboHit = 1; // Mulai combo baru (Hit pertama)
    }
    else
    {
        // Naikkan combo, jika melebihi batas maksimal kembali ke hit 1
        int maxCombo = (currentWeaponData != null) ? currentWeaponData.maxComboCount : 3;
        currentComboHit = (currentComboHit % maxCombo) + 1;
    }

    lastAttackTime = Time.time;

    // Debug log sekarang dijamin menampilkan nama senjata dengan benar karena combo sudah dihitung
    string weaponName = (currentWeaponData != null) ? currentWeaponData.weaponName : "Tangan Kosong";
    Debug.Log($"[Attack Melee] Mengayunkan {weaponName} | Combo Hit: {currentComboHit}");

    // Notify animation system — fires for EVERY click, even when combo value wraps
    OnAttackExecuted?.Invoke(currentComboHit);

    // 3. JALANKAN LOGIKA SERANGAN SENJATA
    if (activeWeaponComponent != null)
    {
        activeWeaponComponent.Attack(); 
    }

    // 4. AKTIFKAN HITBOX SENJATA PALING TERAKHIR AGAR MEMBACA COMBO YANG SUDAH UP-TO-DATE
    if (weaponHitbox != null)
    {
        weaponHitbox.ActivateHitbox();
    }
}

    public void CalculateHitEffects(out int finalDamage, out float stunDuration)
    {
        if (currentWeaponData == null)
        {
            finalDamage = 10 + permanentDamageBuff;
            stunDuration = 0.3f;
            return;
        }

        string category = GetWeaponCategory(currentWeaponData);

        // --- TAMBAHKAN PENGAMAN INI ---
        if (category == "Cannon")
        {
            finalDamage = currentWeaponData.damage;
            stunDuration = 0f; // Cannon biasa tidak nge-stun per hit melee
            return;
        }

        int baseTotal = currentWeaponData.damage + permanentDamageBuff;
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

    private string GetWeaponCategory(WeaponData data)
    {
        if (data == null || string.IsNullOrEmpty(data.weaponName)) return "Unarmed";
        string nameLower = data.weaponName.ToLower();

        if (nameLower.Contains("sword") || nameLower.Contains("blade") || nameLower.Contains("calibur"))
            return "Sword";
        if (nameLower.Contains("hammer") || nameLower.Contains("mallet"))
            return "Hammer";
        if (nameLower.Contains("cannon") || nameLower.Contains("blaster"))
            return "Cannon";
        if (nameLower.Contains("unarmed") || nameLower.Contains("punch") || nameLower.Contains("fist"))
            return "Unarmed";

        return "Unarmed";
    }
    // Tambahkan fungsi ini di dalam PlayerAttack.cs agar datanya bisa diintip oleh BalloonSword
public WeaponData GetCurrentWeaponData()
{
    return currentWeaponData; // Mengembalikan data Scriptable Object yang sedang aktif saat ini[cite: 2]
}
    public void ApplyKnockback(GameObject target)
    {
        if (target == null || currentWeaponData == null) return;

        // Mencari komponen Rigidbody atau EnemyMovement pada musuh untuk memberikan gaya dorong
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // Menghitung arah knockback dari posisi player ke target musuh
            Vector3 knockbackDirection = (target.transform.position - transform.position).normalized;
            knockbackDirection.y = 0; // Biar musuh tidak mental ke atas langit

            // Tentukan kekuatan dorongan (misal: default 5f, atau jika ada data khusus di WeaponData bisa dipakai)
            float force = 5f;

            // Contoh pengondisian jika tipe senjata menentukan kekuatan knockback
            string category = GetWeaponCategory(currentWeaponData);
            if (category == "Hammer") force = 8f; // Palu mendorong lebih kuat

            // Terapkan gaya dorong instan (Impulse)
            targetRb.AddForce(knockbackDirection * force, ForceMode.VelocityChange);
        }
    }
}