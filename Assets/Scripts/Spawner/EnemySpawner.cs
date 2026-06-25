using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Spawners/Enemy Spawner")]
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab musuh yang akan di-spawn")]
    public GameObject enemyPrefab;
   public Transform[] spawnPoints;

    public float spawnRadius = 5f;

    [Tooltip("Interval (detik) antar spawn")]
    public float spawnInterval = 1.5f;

    [Tooltip("Jumlah maksimum enemy aktif sekaligus")]
    public int maxActiveEnemies = 6;

    [Tooltip("Jumlah total enemy yang ingin di-spawn. 0 = tak terbatas")]
    public int totalToSpawn = 0;

    [Tooltip("Mulai otomatis saat Play" )]
    public bool autoStart = true;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnRoutine;
    private int spawnedCount = 0;

    private void Start()
    {
        if (autoStart)
            StartSpawning();
    }

    public void StartSpawning()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: belum assign enemyPrefab");
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

    public void SpawnOnce()
    {
        TrySpawn();
    }

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

        if (totalToSpawn > 0 && spawnedCount >= totalToSpawn) return;
        if (activeEnemies.Count >= maxActiveEnemies) return;

        Vector3 spawnPos = GetSpawnPosition();
        Quaternion rot = Quaternion.identity;

        GameObject go = Instantiate(enemyPrefab, spawnPos, rot);
        activeEnemies.Add(go);
        spawnedCount++;
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform t = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return t.position;
        }

        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(circle.x, 0f, circle.y);
        return pos;
    }

    // Buang referensi null dari list activeEnemies
    private void CleanupList()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
                activeEnemies.RemoveAt(i);
        }
    }

    // Opsional: panggil ini dari enemy saat mati agar spawner tahu langsung
    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var p in spawnPoints)
            {
                if (p == null) continue;
                Gizmos.DrawSphere(p.position, 0.2f);
            }
        }
    }
}