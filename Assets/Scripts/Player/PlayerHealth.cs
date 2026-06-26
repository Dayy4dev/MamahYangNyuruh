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

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"[PlayerHealth] Took {amount} damage. HP: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[PlayerHealth] Healed {currentHealth - before} HP. HP: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died.");
        if (TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<CharacterController>(out var cc)) cc.enabled = false;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.SetTrigger("Die");

        // Matikan auto-reload lama biar diatur sepenuhnya oleh GameManager yang baru
        // StartCoroutine(ReloadSceneAfterDelay());

        // Panggil sistem transisi COOKED di GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    private IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(deathSceneReloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}