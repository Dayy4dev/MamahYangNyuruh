using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Player/Player Health")]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("PlayerHealth: Took " + amount + " damage. Remaining: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the player by the specified amount.
    /// </summary>
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("PlayerHealth: Healed " + amount + " HP. Current: " + currentHealth);
    }

    private void Die()
    {
        Debug.Log("PlayerHealth: Player died");

        // Disable player control scripts if present
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        // Disable CharacterController so player can't be moved by physics
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Play death animation if animator exists
        var animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Disable collider to avoid further hits
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Optionally disable the whole GameObject as a fallback
        // gameObject.SetActive(false);

        // Reload current scene after a short delay
        StartCoroutine(HandleDeathRoutine());
    }

    private IEnumerator HandleDeathRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
