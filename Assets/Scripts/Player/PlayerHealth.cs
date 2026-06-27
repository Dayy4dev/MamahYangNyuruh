using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Player/Player Health")]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Death Settings")]
    [SerializeField] private float deathSceneReloadDelay = 2f;

    // Properti baru untuk melacak status kematian player
    public bool IsDead { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
        IsDead = false; // Reset status saat game mulai
    }

    public void TakeDamage(int amount)
    {
        // Jika sudah mati atau damage tidak valid, abaikan
        if (amount <= 0 || IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"[PlayerHealth] Took {amount} damage. HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        // Jika sudah mati, tidak bisa di-heal
        if (amount <= 0 || IsDead) return;

        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[PlayerHealth] Healed {currentHealth - before} HP. HP: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        if (IsDead) return; // Mencegah fungsi Die terpanggil berkali-kali
        IsDead = true;

        Debug.Log("[PlayerHealth] Player died.");
        
        // 1. Matikan komponen pergerakan dan fisik agar player berhenti di tempat
        if (TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        
        // 2. Jalankan animasi kematian
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Die");

        // 3. Panggil sistem transisi GAME OVER di GameManager kamu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    // Fungsi lama (bisa dihapus jika GameManager sudah menghandle reload scene sepenuhnya)
    private IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(deathSceneReloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}