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

    // Properti untuk melacak status kematian player (dicek oleh PlayerAttack dan PlayerInventory)
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

        Debug.Log("[PlayerHealth] Player died. Freezing game and auto-reloading scene.");
        
        // 1. FIX SUARA JALAN: Cari AudioSource di anak/komponen Player dan matikan suaranya seketika
        // Ini akan menghentikan footstep sound yang terjebak looping karena PlayerMovement dimatikan
        AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in allAudioSources)
        {
            if (audio != null && audio.loop) 
            {
                audio.Stop(); 
            }
        }

        // 2. Matikan komponen pergerakan dan fisik agar player kaku di tempat
        if (TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        
        // 3. Paksa Animator membeku total di pose terakhirnya (Tanpa memicu animasi death)
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) 
        {
            animator.speed = 0f; 
        }

        // 4. BEKUKAN WAKTU GAME TOTAL (Hand Cannon, proyektil, dan musuh langsung mogok)
        Time.timeScale = 0f;

        // 5. Panggil sistem transisi GAME OVER di GameManager (untuk memicu canvas jika ada)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        // 6. Jalankan hitung mundur otomatis tanpa tombol restart
        StartCoroutine(AutoReloadSceneRoutine());
    }

    private IEnumerator AutoReloadSceneRoutine()
    {
        // WAJIB: Gunakan Realtime karena Time.timeScale sedang di posisi 0f
        yield return new WaitForSecondsRealtime(deathSceneReloadDelay);

        // KEMBALIKAN WAKTU KE NORMAL SEBELUM SCENE BARU DIMULAI
        Time.timeScale = 1f;

        // Muat ulang level secara otomatis
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}