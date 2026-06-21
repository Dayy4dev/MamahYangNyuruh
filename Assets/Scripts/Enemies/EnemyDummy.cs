using System.Collections;
using UnityEngine;

public class EnemyDummy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public int maxHealth = 300;
    private int currentHealth;

    [Header("Visual Feedback")]
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.red;

    [Header("UI")]
    public EnemyHealthBar healthBar;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        currentHealth = maxHealth;
        propBlock = new MaterialPropertyBlock();

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            GameObject player = GameObject.FindWithTag("Player");
    if (player != null)
    {
        Collider playerCol = player.GetComponent<Collider>();
        Collider enemyCol = GetComponent<Collider>();
        if (playerCol != null && enemyCol != null)
            Physics.IgnoreCollision(playerCol, enemyCol);
    }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
            StartCoroutine(HitFlash());

        Debug.Log($"{gameObject.name} kena {amount} damage — HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator HitFlash()
    {
        SetColor(hitFlashColor);
        yield return new WaitForSeconds(hitFlashDuration);
        ClearColor();
    }

    private void SetColor(Color color)
    {
        propBlock.SetColor("_BaseColor", color);
        foreach (var smr in skinnedMeshRenderers)
        {
            if (smr == null) continue;
            smr.SetPropertyBlock(propBlock);
        }
    }

    private void ClearColor()
    {
        // Clear property block = kembali ke material asli
        propBlock.Clear();
        foreach (var smr in skinnedMeshRenderers)
        {
            if (smr == null) continue;
            smr.SetPropertyBlock(propBlock);
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} mati!");
        Destroy(gameObject, 0.1f);
    }
}