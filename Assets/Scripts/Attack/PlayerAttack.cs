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
        if (weaponData != null)
        {
            attackDuration = weaponData.attackDuration;
            attackCooldown = weaponData.attackCooldown;
            attackDamage = weaponData.damage;
            knockbackForce = weaponData.knockbackForce;
            knockbackDuration = weaponData.knockbackDuration;
        }
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

        if (Input.GetMouseButton(1))
        {
            if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f && !isAttacking)
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
        weaponHitbox.Deactivate();
    }

    // Dipanggil dari WeaponHitbox saat mengenai enemy
    public void ApplyKnockback(GameObject target)
    {
        // Cari di root object, bukan di child yang kena hit
        GameObject root = target.transform.root.gameObject;
        
        Vector3 knockbackDir = (root.transform.position - transform.position).normalized;
        knockbackDir.y = 0f;

        if (root.TryGetComponent<IKnockbackable>(out IKnockbackable knockbackable))
        {
            knockbackable.TakeKnockback(knockbackDir, knockbackForce, knockbackDuration);
        }
        else if (root.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }
    }
}