using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public int spawnCount = 3;
    public float spawnRadius = 5f; // Radius area spawn acak

    [Header("Layer Mask Setup")]
    public LayerMask groundLayer;  // Set ke layer lantai/ground di Inspector

    // Fungsi utama untuk men-spawn musuh tanpa collider
    public void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"[EnemySpawner] Enemy Prefab belum di-assign di {gameObject.name}!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            // 1. Ambil koordinat acak di dalam radius lingkaran (X dan Z)
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(
                transform.position.x + randomCircle.x, 
                transform.position.y, 
                transform.position.z + randomCircle.y
            );

            // 2. Gunakan Raycast ke bawah untuk memastikan musuh menapak di atas tanah/lantai
            if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                spawnPos = hit.point;
            }

            // 3. Spawn musuh dan masukkan sebagai child dari Room agar terdeteksi
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity, transform.parent);
            
            // Set tag musuh agar sesuai dengan deteksi RoomController
            enemy.tag = "Enemy"; 
        }

        Debug.Log($"[EnemySpawner] Berhasil spawn {spawnCount} musuh di {transform.parent.name}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FIX TAMBAHAN: Fungsi Callback saat musuh mati/hancur
    // ─────────────────────────────────────────────────────────────────────────
    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        Debug.Log($"[EnemySpawner] Musuh {enemy.name} melaporkan kematian.");
        
        // Di sini kamu bisa menambahkan logika tambahan jika dibutuhkan kelak, 
        // misalnya menghitung sisa musuh wave di spawner, memicu UI counter, dll.
    }

    // Menggambar area jangkauan spawn di Unity Editor (tidak muncul di game)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}