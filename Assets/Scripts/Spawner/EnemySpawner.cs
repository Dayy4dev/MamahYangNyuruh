using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;              // hapus jika pakai Text biasa
using UnityEngine.UI;     // untuk Image (opsional)

[AddComponentMenu("Spawners/Enemy Spawner")]
[RequireComponent(typeof(Collider))]
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnRadius = 5f;
    public float spawnInterval = 1.5f;
    public int maxActiveEnemies = 6;
    public int totalToSpawn = 0;

    [Header("Player Detection")]
    public string playerTag = "Player";
    public bool stopOnPlayerExit = false;
    public bool resetCountOnReenter = false;

    [Header("UI")]
    [Tooltip("Panel UI yang muncul saat player masuk area")]
    public GameObject hudPanel;

    [Tooltip("Text untuk jumlah musuh aktif. Format: '4 / 6'")]
    public TMP_Text activeCountText;      // ganti ke Text jika tidak pakai TMPro

    [Tooltip("(Opsional) Text untuk total musuh yang sudah di-spawn")]
    public TMP_Text totalSpawnedText;

    [Tooltip("(Opsional) Text label nama area/room")]
    public TMP_Text areaNameText;

    [Tooltip("Nama area ini, ditampilkan di UI")]
    public string areaName = "Area";

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnRoutine;
    private int spawnedCount = 0;
    private bool playerInside = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        SetHudVisible(false);
    }

    // ── Trigger ───────────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || playerInside) return;
        playerInside = true;

        if (resetCountOnReenter) spawnedCount = 0;

        SetHudVisible(true);
        if (areaNameText != null) areaNameText.text = areaName;
        RefreshUI();
        StartSpawning();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;

        if (stopOnPlayerExit) StopSpawning();
        SetHudVisible(false);
    }

    // ── Spawning ──────────────────────────────────────────────────────────────

    public void StartSpawning()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab belum di-assign!");
            return;
        }
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void SpawnOnce() => TrySpawn();

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            TrySpawn();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void TrySpawn()
    {
        CleanupList();

        if (totalToSpawn > 0 && spawnedCount >= totalToSpawn)
        {
            StopSpawning();
            return;
        }
        if (activeEnemies.Count >= maxActiveEnemies) return;

        Vector3 pos = GetSpawnPosition();
        GameObject go = Instantiate(enemyPrefab, pos, Quaternion.identity);
        activeEnemies.Add(go);
        spawnedCount++;

        RefreshUI();
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;

        Vector2 c = Random.insideUnitCircle * spawnRadius;
        return transform.position + new Vector3(c.x, 0f, c.y);
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    private void SetHudVisible(bool visible)
    {
        if (hudPanel != null)
            hudPanel.SetActive(visible);
    }

    private void RefreshUI()
    {
        if (activeCountText != null)
            activeCountText.text = $"{activeEnemies.Count} / {maxActiveEnemies}";

        if (totalSpawnedText != null)
        {
            if (totalToSpawn > 0)
                totalSpawnedText.text = $"Total spawn: {spawnedCount} / {totalToSpawn}";
            else
                totalSpawnedText.text = $"Total spawn: {spawnedCount}";
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    private void CleanupList()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
            if (activeEnemies[i] == null) activeEnemies.RemoveAt(i);
    }

    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        RefreshUI();        // ← update UI saat musuh mati
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var p in spawnPoints)
            {
                if (p != null) Gizmos.DrawSphere(p.position, 0.2f);
            }
        }

        Gizmos.color = playerInside
            ? new Color(0f, 1f, 0f, 0.15f)
            : new Color(1f, 0f, 0f, 0.1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;
    }
}