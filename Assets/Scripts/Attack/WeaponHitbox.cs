using UnityEngine;
using System.Collections;

public class WeaponHitbox : MonoBehaviour
{
    private Collider hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; 
        }
        // HAPUS pencarian playerAttack di sini agar tidak error Null lagi!
    }

    public void ActivateHitbox()
    {
        StopAllCoroutines();
        StartCoroutine(HitboxRoutine());
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
        if (hitboxCollider != null) hitboxCollider.enabled = true; 
        yield return new WaitForSeconds(0.2f);
        if (hitboxCollider != null) hitboxCollider.enabled = false; 
    }

    private void OnTriggerEnter(Collider other)
    {
        // Cegah melukai diri sendiri
        if (other.CompareTag("Player")) return;

        IDamageable enemyHealth = other.GetComponent<IDamageable>();

        if (enemyHealth != null)
        {
            // 1. Cari PlayerAttack secara dinamis dari parent saat terjadi hit
            PlayerAttack playerAttack = GetComponentInParent<PlayerAttack>();

            // 2. Ambil data damage dari script BalloonSword terdekat
            BalloonSword sword = GetComponentInParent<BalloonSword>();
            int damageYangDiberikan = 0;

            if (sword != null)
            {
                damageYangDiberikan = sword.GetDamageValue();
            }
            else if (playerAttack != null)
            {
                playerAttack.CalculateHitEffects(out damageYangDiberikan, out _);
            }

            // Jika damage masih 0/gagal ke-load, beri backup angka 10 agar game tidak nge-bug
            if (damageYangDiberikan <= 0) damageYangDiberikan = 10;

            // 3. Berikan damage ke musuh!
            enemyHealth.TakeDamage(damageYangDiberikan);
            Debug.Log($"[Hitbox] Berhasil memberikan Damage: {damageYangDiberikan} ke {other.name}");

            // 4. Berikan efek dorongan (Knockback) jika playerAttack ditemukan
            if (playerAttack != null)
            {
                playerAttack.ApplyKnockback(other.gameObject);
            }

            if (other.TryGetComponent<EnemyStunHandler>(out var stunHandler))
            {
                stunHandler.TriggerStun(0.3f);
            }
        }
    }
}