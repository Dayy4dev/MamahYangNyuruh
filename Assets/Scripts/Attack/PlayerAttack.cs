using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private WeaponData weaponData;

    private int attackDamage;
    private float attackDuration;
    private float attackCooldown;
    private float knockbackForce;
    private float knockbackDuration;

    private float attackTimer;
    private float cooldownTimer;
    private bool isAttacking;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        if (weaponData != null && weaponHitbox != null)
            EquipWeapon(weaponHitbox, weaponData);
    }

    void Update()
    {
        TickAttack();
        TickCooldown();
        HandleInput();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void EquipWeapon(WeaponHitbox newHitbox, WeaponData newData)
    {
        weaponHitbox?.Deactivate();

        weaponHitbox    = newHitbox;
        weaponData      = newData;
        attackDuration  = weaponData.attackDuration;
        attackCooldown  = weaponData.attackCooldown;
        attackDamage    = weaponData.damage;
        knockbackForce  = weaponData.knockbackForce;
        knockbackDuration = weaponData.knockbackDuration;

        Debug.Log($"[PlayerAttack] Equipped: {weaponData.name}");
    }

    public void ApplyKnockback(GameObject target)
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir.y = 0f;

        if (target.TryGetComponent<IKnockbackable>(out IKnockbackable knockbackable))
            knockbackable.TakeKnockback(dir, knockbackForce, knockbackDuration);
        else if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void HandleInput()
    {
        // Tahan klik kanan untuk aiming, klik kiri untuk menyerang
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

        isAttacking  = true;
        attackTimer  = attackDuration;
        cooldownTimer = attackCooldown;
        weaponHitbox.Activate(attackDamage);
    }

    private void EndAttack()
    {
        isAttacking = false;
        weaponHitbox?.Deactivate();
    }
}