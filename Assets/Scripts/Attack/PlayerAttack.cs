using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private WeaponHitbox weaponHitbox;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private WeaponData currentWeaponData;
    [Header("Ranged Setup")]
[SerializeField] private GameObject bulletPrefab; // Tarif prefab Bullet.cs kamu ke sini nanti
[SerializeField] private Transform firePoint;     // Ujung laras tembakan HandCannon

    private int   attackDamage;
    private float attackDuration;
    private float attackCooldown;
    private float knockbackForce;
    private float knockbackDuration;

    private float attackTimer;
    private float cooldownTimer;
    private bool  isAttacking;
    private bool  isArmed; // false = unarmed, input attack diabaikan

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
    /// data == null  →  Unarmed (tidak bisa attack)
    /// data != null  →  Equipped weapon
    /// </summary>
  public void EquipWeaponData(WeaponData data)
{
    EndAttack(); // Menghentikan serangan sebelumnya jika ada

    currentWeaponData = data;
    isArmed           = data != null;

    if (isArmed)
    {
        attackDamage      = data.damage;
        attackDuration    = data.attackDuration;
        attackCooldown    = data.attackCooldown;
        knockbackForce    = data.knockbackForce;
        knockbackDuration = data.knockbackDuration;
        
        Debug.Log($"[PlayerAttack] Berhasil Sinkron Data: {data.weaponName}");
    }
    else
    {
        Debug.Log("[PlayerAttack] Tidak memegang senjata (Unarmed)");
    }
}
    // TAMBAHKAN fungsi baru ini di dalam kelas PlayerAttack:
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

    // CEK: Apakah senjata yang dipegang saat ini adalah HandCannon?
    if (currentWeaponData != null && (currentWeaponData.weaponName == "Hand_Cannon" || currentWeaponData.weaponName == "HandCannon"))
    {
        // PANGGIL fungsi menembak bawaan asli kamu di sini!
        // Contoh nama fungsi menembak aslimu (sesuaikan jika namanya berbeda):
        FireRangedWeapon(); 
        
        Debug.Log("[PlayerAttack] HandCannon menembak!");
    }
    else
    {
        // JIKA BUKAN HANDCANNON (Berarti ToyHammer / Melee)
        if (weaponHitbox == null)
        {
            Debug.LogWarning("[PlayerAttack] WeaponHitbox belum terpasang untuk senjata Melee!");
            return;
        }
        weaponHitbox.Activate(attackDamage);
    }
}
// Tambahkan fungsi baru ini di bawah StartAttack
private void FireRangedWeapon()
{
    if (bulletPrefab == null)
    {
        Debug.LogError("[PlayerAttack] Bullet Prefab belum dimasukkan di Inspector!");
        return;
    }

    // Tentukan posisi menembak (pakai firePoint, kalau kosong pakai posisi player agak maju)
    Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 1f;
    Quaternion spawnRot = transform.rotation;

    // Spawn peluru ke dunia game
    GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, spawnRot);

    // Cari komponen Bullet di objek peluru tersebut, lalu beri kecepatan & damage
    if (bulletObj.TryGetComponent<Bullet>(out Bullet bullet))
    {
        // Sesuaikan parameter ini dengan apa yang ada di script Bullet.cs milikmu
        // Misal biasanya: bullet.Setup(currentWeaponData.speed, attackDamage);
        
        // Catatan: Jika script Bullet kamu menggunakan Rigidbody untuk bergerak, kodenya:
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