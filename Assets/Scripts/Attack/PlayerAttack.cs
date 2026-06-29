using UnityEngine;

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

    public void ExecuteAttack()
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (currentWeaponData == null) return;

        string weaponCategory = GetWeaponCategory(currentWeaponData);

        // --- JALUR KHUSUS SENJATA JARAK JAUH (CANNON) ---
        if (weaponCategory == "Cannon")
        {
            ResetCombo(); // Cannon tidak punya combo
            if (activeWeaponComponent != null)
            {
                // Panggil fungsi Attack bawaan HandCannon kamu agar peluru keluar
                activeWeaponComponent.Attack(); 
            }
            return;
        }

        // --- JALUR KHUSUS SENJATA JARAK DEKAT (SWORD / HAMMER / UNARMED) ---
        if (Time.time - lastAttackTime > comboResetDuration)
        {
            ResetCombo();
        }

        currentComboHit++; 

        if (weaponCategory == "Sword" && currentComboHit > 3) currentComboHit = 1;
        if (weaponCategory == "Hammer" && currentComboHit > 2) currentComboHit = 1;
        if (weaponCategory == "Unknown") currentComboHit = 1; 

        lastAttackTime = Time.time;

        Debug.Log($"[Attack Melee] Mengayunkan {currentWeaponData.weaponName} | Combo Hit: {currentComboHit}");

        // Aktifkan komponen penyerang melee bawaan jika ada
        if (activeWeaponComponent != null)
        {
            activeWeaponComponent.Attack();
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

        // AMBIL AMAN: Selalu ambil data damage asli dari scriptable object senjata aktif!
        int baseTotal = currentWeaponData.damage + permanentDamageBuff;
        finalDamage = baseTotal;
        stunDuration = 0.3f; 

        string category = GetWeaponCategory(currentWeaponData);

        if (category == "Sword" && currentComboHit == 3)
        {
            finalDamage = Mathf.RoundToInt(baseTotal * 1.20f); // +20% Damage Finisher
        }
        else if (category == "Hammer" && currentComboHit == 2)
        {
            finalDamage = Mathf.RoundToInt(baseTotal * 1.25f); // +25% Damage Finisher
            stunDuration = 0.8f; 
        }
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