using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Player/Player Health")]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Variables")]
    [SerializeField] private int maxHealth = 100;    // Batas maksimal HP awal player
    private int currentHealth;                       // Darah player saat ini

    [Header("Death Settings")]
    [SerializeField] private float deathSceneReloadDelay = 2f;

    // Properti untuk melacak status kematian player (dicek oleh PlayerAttack dan PlayerInventory)
    public bool IsDead { get; private set; }

    // Fungsi publik agar script lain (seperti HPBar & Partner) bisa mengambil angka darah saat ini
    public int GetMaxHealth()
    {
        return maxHealth; 
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

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

    public void IncreaseMaxHealth(int amount)
    {
        if (IsDead) return;

        maxHealth += amount; // Ditambah 30 dari luar
        
        // Hitung bonus heal 10% dari New Max HP
        int bonusHeal = Mathf.RoundToInt(maxHealth * 0.10f);
        
        // Terapkan heal, batasi agar tidak melebihi maxHealth baru
        currentHealth = Mathf.Min(currentHealth + bonusHeal, maxHealth);

        Debug.Log($"[Partner Effect] Max HP bertambah {amount}! Bonus Heal 10% dari Max HP baru (+{bonusHeal}). HP Sekarang: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        if (IsDead) return; // Mencegah fungsi Die terpanggil berkali-kali
        IsDead = true;

        Debug.Log("[PlayerHealth] Player died. Freezing game and auto-reloading scene.");

        // Sembunyikan UI Room Bar saat player mati
        if (DungeonManager.Instance != null && DungeonManager.Instance.roomUIBar != null)
        {
            DungeonManager.Instance.roomUIBar.ShowBar(false);
        }

        // FIX SUARA JALAN: Cari AudioSource di anak/komponen Player dan matikan suaranya seketika
        AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audio in allAudioSources)
        {
            if (audio != null && audio.loop)
            {
                audio.Stop();
            }
        }

        // Matikan komponen pergerakan dan fisik agar player kaku di tempat
        if (TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;

        // Paksa Animator membeku total di pose terakhirnya
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.speed = 0f;
        }

        // BEKUKAN WAKTU GAME TOTAL
        Time.timeScale = 0f;

        // Panggil sistem transisi GAME OVER di GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        // Jalankan hitung mundur otomatis
        StartCoroutine(AutoReloadSceneRoutine());
    }

    private IEnumerator AutoReloadSceneRoutine()
    {
        yield return new WaitForSecondsRealtime(deathSceneReloadDelay);

        // KEMBALIKAN WAKTU KE NORMAL SEBELUM SCENE BARU DIMULAI
        Time.timeScale = 1f;

        // Muat ulang level secara otomatis
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}