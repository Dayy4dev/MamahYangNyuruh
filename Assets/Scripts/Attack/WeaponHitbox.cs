using UnityEngine;
using System.Collections;

public class WeaponHitbox : MonoBehaviour
{
    private Collider hitboxCollider;

    void Awake()
    {
        // Mengambil komponen collider yang ada di objek hitbox/pedang ini
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; // Nonaktifkan di awal agar tidak ngehit musuh tanpa sengaja
        }
    }

    // Fungsi ini dipanggil dari PlayerAttack saat klik kiri dilakukan
    public void ActivateHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
        }
    }
    public void DeactivateHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    private IEnumerator HitboxRoutine()
    {
        if (hitboxCollider != null) hitboxCollider.enabled = true; // Aktifkan collider untuk mendeteksi musuh

        // Biarkan hitbox aktif selama 0.2 detik (durasi tebasan pedang)
        yield return new WaitForSeconds(0.2f);

        if (hitboxCollider != null) hitboxCollider.enabled = false; // Matikan kembali collider
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Deteksi musuh lewat komponen IDamageable
        IDamageable enemyHealth = other.GetComponent<IDamageable>();

        if (enemyHealth != null)
        {
            PlayerAttack playerAttack = Object.FindFirstObjectByType<PlayerAttack>();

            if (playerAttack != null)
            {
                // 2. Hitung damage combo + durasi stun dari PlayerAttack
                playerAttack.CalculateHitEffects(out int finalDamage, out float stunDuration);

                // 3. Berikan damage ke musuh
                enemyHealth.TakeDamage(finalDamage);

                // 4. Berikan efek knockback bawaan
                playerAttack.ApplyKnockback(other.gameObject);

                // 5. Berikan efek Stun jika musuh punya komponen EnemyStunHandler
                if (other.TryGetComponent<EnemyStunHandler>(out var stunHandler))
                {
                    stunHandler.TriggerStun(stunDuration);
                }
            }
        }
    }
}