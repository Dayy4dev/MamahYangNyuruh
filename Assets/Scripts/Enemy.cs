using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 300; // Menggunakan nilai 300 dari script kedua
    private int currentHealth;

    [Header("Visual Feedback")]
    public Renderer bodyRenderer;
    public float hitFlashDuration = 0.1f;
    private Color originalColor;

    [Header("UI")]
    // Gunakan EnemyHealthBar (atau ubah menjadi HealthBar jika itu tipe script UI Anda)
    public EnemyHealthBar healthBar; // optional, assign di inspector

    void Start()
    {
        currentHealth = maxHealth;

        if (bodyRenderer != null)
            originalColor = bodyRenderer.material.color;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        // Flash merah saat kena hit
        if (bodyRenderer != null)
            StartCoroutine(HitFlash());

        Debug.Log($"{gameObject.name} kena {amount} damage — HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HitFlash()
    {
        // Pengecekan null agar tidak error jika renderer hilang saat dipanggil
        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = Color.red;
            yield return new WaitForSeconds(hitFlashDuration);
            bodyRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} mati!");
        // Tambahkan efek mati di sini (partikel, sound, dll)
        Destroy(gameObject, 0.1f);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Fungsi dari script kedua untuk mengabaikan collision dengan Player
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}