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
    [SerializeField] private AudioClip[] enemyHurtSounds = new AudioClip[3]; // 3 variasi suara hurt
    [SerializeField] private AudioClip enemyDeathSound; // Suara saat musuh ini mati

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);

        // Daftar ke spawner terdekat agar counter UI terupdate
        spawner = GetComponentInParent<EnemySpawner>();
        if (spawner == null)
            spawner = FindFirstObjectByType<EnemySpawner>();

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
    if (currentHealth <= 0) return; // Sudah mati, abaikan

    currentHealth -= amount;
    Debug.Log($"{gameObject.name} terkena {amount} damage. HP tersisa: {currentHealth}");

    if (healthBar != null)
        healthBar.SetHealth(currentHealth);

    // Efek flash warna merah
    StartCoroutine(HitFlash());

    // ==========================================
    // 🔴 PERBAIKAN: PLAY RANDOM HURT SOUND
    // ==========================================
    if (audioSource != null && currentHealth > 0)
    {
        // Pilih suara hurt random dari array
        AudioClip randomHurtSound = enemyHurtSounds[Random.Range(0, enemyHurtSounds.Length)];
        if (randomHurtSound != null)
        {
            audioSource.PlayOneShot(randomHurtSound);
        }
    }
    // ==========================================

    if (currentHealth <= 0)
    {
        Die();
    }
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

    // 🔴 PASTIKAN BARIS INI ADA DI SINI SEBELUM DESTROY!
    if (TutorialManager.Instance != null)
    {
        TutorialManager.Instance.enemiesDefeated++;
    }

    float destroyDelay = 0.1f;
    if (audioSource != null && enemyDeathSound != null)
    {
        audioSource.PlayOneShot(enemyDeathSound);
        // Kasih waktu suara mati untuk selesai dulu sebelum objek dihancurkan.
        destroyDelay = Mathf.Max(destroyDelay, enemyDeathSound.length);
    }

    Destroy(gameObject, destroyDelay);
}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}