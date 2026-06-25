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

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private Color[][] originalColors;

    private string colorPropertyName = "_BaseColor";

    [Header("UI")]
    public EnemyHealthBar healthBar;

    private EnemySpawner spawner;

    // -------------------------------------------------------------------------
    // TAMBAHAN: Audio Setup untuk Musuh / Dummy Kesakitan
    // -------------------------------------------------------------------------
    [Header("Audio Setup")]
    [SerializeField] private AudioSource audioSource;   // Komponen AudioSource milik Enemy/Dummy
    [SerializeField] private AudioClip enemyHurtSound; // File suara placeholder musuh mengaduh

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        // Daftar ke spawner terdekat agar counter UI terupdate
        spawner = GetComponentInParent<EnemySpawner>();
        if (spawner == null)
            spawner = FindObjectOfType<EnemySpawner>();

        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        originalColors = new Color[skinnedMeshRenderers.Length][];

        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            Material[] materials = skinnedMeshRenderers[i].materials;
            originalColors[i] = new Color[materials.Length];
            
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty(colorPropertyName))
                {
                    originalColors[i][j] = materials[j].GetColor(colorPropertyName);
                }
            }
        }
    }

    public void TakeDamage(int amount)
{
    currentHealth -= amount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

    if (healthBar != null)
        healthBar.SetHealth(currentHealth);

    if (audioSource != null && enemyHurtSound != null)
        audioSource.PlayOneShot(enemyHurtSound);

    if (skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0)
    {
        StartCoroutine(HitFlash());
    }

    // --- KNOCKBACK ---
    // Cari objek player di dalam map
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    {

        if (player.TryGetComponent<PlayerAttack>(out PlayerAttack pAttack))
        {
            pAttack.ApplyKnockback(gameObject);
        }
    }
    // ------------------------------------------

    Debug.Log($"{gameObject.name} kena {amount} damage — HP: {currentHealth}/{maxHealth}");

    if (currentHealth <= 0)
        Die();
} 

    IEnumerator HitFlash()
    {
        ChangeMeshColor(hitFlashColor);
        
        yield return new WaitForSeconds(hitFlashDuration);
        
        ResetMeshColor();
    }

    private void ChangeMeshColor(Color color)
    {
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            if (skinnedMeshRenderers[i] == null) continue;

            Material[] materials = skinnedMeshRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty(colorPropertyName))
                {
                    materials[j].SetColor(colorPropertyName, color);
                }
            }
        }
    }

    private void ResetMeshColor()
    {
        for (int i = 0; i < skinnedMeshRenderers.Length; i++)
        {
            if (skinnedMeshRenderers[i] == null) continue;

            Material[] materials = skinnedMeshRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j].HasProperty(colorPropertyName))
                {
                    materials[j].SetColor(colorPropertyName, originalColors[i][j]);
                }
            }
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} mati!");

        if (spawner != null)
            spawner.NotifyEnemyDestroyed(gameObject);

        Destroy(gameObject, 0.1f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}