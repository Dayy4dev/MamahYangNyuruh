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
        EndAttack();

        currentWeaponData = data;
        isArmed           = data != null;

        if (isArmed)
        {
            attackDamage      = data.damage;
            attackDuration    = data.attackDuration;
            attackCooldown    = data.attackCooldown;
            knockbackForce    = data.knockbackForce;
            knockbackDuration = data.knockbackDuration;
            Debug.Log($"[PlayerAttack] Equipped: {data.weaponName}");
        }
        else
        {
            Debug.Log("[PlayerAttack] Equipped: Unarmed");
        }
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
        if (weaponHitbox == null)
        {
            Debug.LogWarning("[PlayerAttack] WeaponHitbox belum di-assign!");
            return;
        }

        isAttacking   = true;
        attackTimer   = attackDuration;
        cooldownTimer = attackCooldown;
        weaponHitbox.Activate(attackDamage);
    }

    private void EndAttack()
    {
        isAttacking = false;
        weaponHitbox?.Deactivate();
    }
}