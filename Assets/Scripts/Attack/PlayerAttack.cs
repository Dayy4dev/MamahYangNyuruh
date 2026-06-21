using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    public WeaponHitbox weaponHitbox;
    [SerializeField] private WeaponData weaponData;

    private float knockbackForce;
    private float knockbackDuration;

    private float attackDuration;
    private float attackCooldown;
    private int attackDamage;
    private float attackTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isAttacking = false;

    void Start()
    {
        // Tetap inisialisasi senjata awal jika di Inspector sudah diisi
        if (weaponData != null && weaponHitbox != null)
        {
            EquipWeapon(weaponHitbox, weaponData);
        }
    }

    // --- FUNGSI BARU UNTUK GANTI SENJATAsecara DINAMIS ---
    public void EquipWeapon(WeaponHitbox newHitbox, WeaponData newData)
    {
        // 1. Matikan hitbox senjata yang lama jika ada
        if (weaponHitbox != null)
        {
            weaponHitbox.Deactivate();
        }

        // 2. Set referensi ke senjata yang baru
        weaponHitbox = newHitbox;
        weaponData = newData;

        // 3. Update semua stats sesuai senjata baru yang dipakai
        attackDuration = weaponData.attackDuration;
        attackCooldown = weaponData.attackCooldown;
        attackDamage = weaponData.damage;
        knockbackForce = weaponData.knockbackForce;
        knockbackDuration = weaponData.knockbackDuration;

        Debug.Log($"Berhasil menggunakan senjata baru: {weaponData.name}");
    }

    void Update()
    {
        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                EndAttack();
            }
        }

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButton(1)) // Klik kanan ditahan
        {
            if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f && !isAttacking) // Klik kiri untuk hit
            {
                StartAttack();
            }
        }
    }

    void StartAttack()
    {
        if (weaponHitbox == null)
        {
            Debug.LogWarning("WeaponHitbox belum di-assign!");
            return;
        }
        
        isAttacking = true;
        attackTimer = attackDuration;
        cooldownTimer = attackCooldown;
        weaponHitbox.Activate(attackDamage);
    }

    void EndAttack()
    {
        isAttacking = false;
        if (weaponHitbox != null) weaponHitbox.Deactivate();
    }

    public void ApplyKnockback(GameObject target)
    {
        Vector3 knockbackDir = (target.transform.position - transform.position).normalized;
        knockbackDir.y = 0f; 

        if (target.TryGetComponent<IKnockbackable>(out IKnockbackable knockbackable))
        {
            knockbackable.TakeKnockback(knockbackDir, knockbackForce, knockbackDuration);
        }
        else if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }
    }
}