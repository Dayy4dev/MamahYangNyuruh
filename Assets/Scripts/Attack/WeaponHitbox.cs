using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : Weapon
{
    [Header("Hit Detection")]
    [SerializeField] private Collider hitCollider;

    [Tooltip("Layer yang bisa kena damage (centang Enemy saja, jangan Player)")]
    [SerializeField] private LayerMask targetLayers;

    private int damage;
    private bool isActive;
    private HashSet<Collider> hitThisSwing = new HashSet<Collider>();
    private PlayerAttack playerAttack;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        if (hitCollider == null)
            hitCollider = GetComponent<Collider>();

        hitCollider.isTrigger = true;
        hitCollider.enabled = false;

        playerAttack = GetComponentInParent<PlayerAttack>();

        if (playerAttack == null)
            Debug.LogWarning("[WeaponHitbox] PlayerAttack tidak ditemukan di parent!");
    }

    // -------------------------------------------------------------------------
    // Weapon Overrides
    // -------------------------------------------------------------------------

    public override void Attack() => Activate(damage);

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

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
        hitThisSwing.Clear();
    }

    // -------------------------------------------------------------------------
    // Trigger
    // -------------------------------------------------------------------------

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (hitThisSwing.Contains(other)) return;

        // FIX: hanya mengenai layer yang ada di targetLayers (misal: Enemy)
        // Ini mencegah player kena damage dari senjatanya sendiri
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (!other.TryGetComponent<IDamageable>(out IDamageable target)) return;

        hitThisSwing.Add(other);
        target.TakeDamage(damage);

        if (playerAttack != null)
            playerAttack.ApplyKnockback(other.gameObject);
    }
}