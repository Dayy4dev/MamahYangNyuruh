using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private Collider hitCollider;
    private int damage;
    private bool isActive = false;
    private HashSet<Collider> hitThisSwing = new HashSet<Collider>();

    void Awake()
    {
        hitCollider = GetComponent<Collider>();
        hitCollider.isTrigger = true;
        hitCollider.enabled = false;
    }

    public void Activate(int dmg)
    {
        damage = dmg;
        isActive = true;
        hitThisSwing.Clear();        // reset daftar yang sudah kena
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
        if (hitThisSwing.Contains(other)) return;   // sudah kena, skip

        // Cek apakah target memiliki IDamageable
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            hitThisSwing.Add(other);
            target.TakeDamage(damage);
        }
    }
}