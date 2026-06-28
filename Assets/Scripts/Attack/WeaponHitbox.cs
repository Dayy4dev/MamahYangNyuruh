using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : Weapon
{
    [Header("Hit Detection")]
    [SerializeField] private Collider hitCollider;

    [Tooltip("Layer yang bisa kena damage (centang Enemy saja, jangan Player)")]
    [SerializeField] private LayerMask targetLayers;

    // -------------------------------------------------------------------------
    // TAMBAHAN: Audio Setup untuk Hitbox Senjata
    // -------------------------------------------------------------------------
    [Header("Audio Setup")]
    [SerializeField] private AudioSource audioSource;   // Komponen AudioSource Player/Weapon
    [SerializeField] private AudioClip hitWallSound;    // Suara keras benturan keras (Tag: Wall)
    [SerializeField] private AudioClip hitEnemySound;   // Suara benturan mengenai daging (Tag: Enemy)


    private int damage;
    private bool isActive;
    private bool hasHitSomething; // Melacak apakah ayunan ini mengenai sesuatu
    private HashSet<Collider> hitThisSwing = new HashSet<Collider>();
    private PlayerAttack playerAttack;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        if (hitCollider == null)
            hitCollider = GetComponent<Collider>();

        if (hitCollider == null)
        {
            Debug.LogError("[WeaponHitbox] No Collider found on this object!");
            return;
        }

        hitCollider.isTrigger = true;
        hitCollider.enabled = false;

        playerAttack = GetComponentInParent<PlayerAttack>();
        if (playerAttack == null)
        {
            playerAttack = GameObject.FindWithTag("Player")?.GetComponent<PlayerAttack>();
        }
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
        if (hitCollider == null) return;
        
        damage = dmg;
        isActive = true;
        hasHitSomething = false; // Reset status setiap kali ayunan baru dimulai
        hitThisSwing.Clear();
        hitCollider.enabled = true;
    }

    public void Deactivate()
    {
        if (hitCollider == null) return;
        
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

        // 1. CEK BENTURAN DENGAN WALL (Dinding)
        // Dinding biasanya tidak punya IDamageable, jadi kita cek Tag-nya di paling atas
        if (other.CompareTag("Wall"))
        {
            if (hitThisSwing.Contains(other)) return;
            hitThisSwing.Add(other);
            
            hasHitSomething = true; // Tandai bahwa ayunan mengenai sesuatu

            if (audioSource != null && hitWallSound != null)
                audioSource.PlayOneShot(hitWallSound);

            Debug.Log("[WeaponHitbox] Mengenai Dinding!");
            return; // Keluar karena dinding tidak menerima damage/knockback
        }

        // 2. CEK BENTURAN DENGAN LAYER TARGET (Misal: Enemy)
        if (hitThisSwing.Contains(other)) return;
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0) return;
        if (!other.TryGetComponent<IDamageable>(out IDamageable target)) return;

        hitThisSwing.Add(other);
        hasHitSomething = true; // Tandai bahwa ayunan mengenai sesuatu

        // Putar suara benturan musuh
        if (audioSource != null && hitEnemySound != null)
            audioSource.PlayOneShot(hitEnemySound);

        target.TakeDamage(damage);

        if (playerAttack != null)
            playerAttack.ApplyKnockback(other.gameObject);
            
        Debug.Log($"[WeaponHitbox] Mengenai Target Valid: {other.name}");
    }
}