using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDuration = 0.25f;
    public float attackCooldown = 0.75f;
    public int attackDamage = 20;

    [Header("References")]
    public WeaponHitbox weaponHitbox;

    private float attackTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isAttacking = false;

    void Update()
    {
        // Hitung timer
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
        isAttacking = true;
        attackTimer = attackDuration;
        cooldownTimer = attackCooldown;

        // Aktifkan hitbox — arah sudah mengikuti rotasi karakter
        weaponHitbox.Activate(attackDamage);
        Debug.Log("Attack");

    }

    void EndAttack()
    {
        isAttacking = false;
        weaponHitbox.Deactivate();
    }
}