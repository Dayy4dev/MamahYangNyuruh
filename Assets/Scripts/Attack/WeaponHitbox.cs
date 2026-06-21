using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : Weapon
{
    [SerializeField] private Collider hitCollider;
    private int damage;
    private bool isActive = false;
    private HashSet<Collider> hitThisSwing = new HashSet<Collider>();

    private PlayerAttack playerAttack;

    void Awake()
    {
        hitCollider = GetComponent<Collider>();
        hitCollider.isTrigger = true;
        hitCollider.enabled = false;

        // Ambil PlayerAttack dari parent untuk knockback
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    public override void Attack()
    {
        Activate(damage);
    }

    public void Activate(int dmg)
    {
        damage = dmg;
        isActive = true;
        hitThisSwing.Clear();
        hitCollider.enabled = true;
    }

    public void Deactivate()
    {
        isActive = false;
        hitCollider.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (hitThisSwing.Contains(other)) return;
        if (other.CompareTag("Player")) return; // tambah ini — ignore player sendiri

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            hitThisSwing.Add(other);
            target.TakeDamage(damage);

            if (playerAttack != null)
                playerAttack.ApplyKnockback(other.gameObject);
        }
    }
}