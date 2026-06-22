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

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // -------------------------------------------------------------------------
    // IDamageable
    // -------------------------------------------------------------------------

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"[PlayerHealth] Took {amount} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("PlayerHealth: Healed " + amount + " HP. Current: " + currentHealth);
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player died.");

        // Matikan kontrol gerakan
        if (TryGetComponent<PlayerMovement>(out PlayerMovement movement))
            movement.enabled = false;

        // Matikan CharacterController agar tidak terpengaruh fisika
        if (TryGetComponent<CharacterController>(out CharacterController cc))
            cc.enabled = false;

        // Matikan collider agar tidak kena hit lagi
        if (TryGetComponent<Collider>(out Collider col))
            col.enabled = false;

        // Trigger animasi mati jika ada
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.SetTrigger("Die");

        StartCoroutine(ReloadSceneAfterDelay());
    }

    private IEnumerator ReloadSceneAfterDelay()
    {
        yield return new WaitForSeconds(deathSceneReloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}