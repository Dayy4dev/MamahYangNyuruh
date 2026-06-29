using UnityEngine;

public class LootboxRoomSpawner : MonoBehaviour
{
    [Header("Pilihan Peti yang Bisa Muncul")]
    public GameObject[] chestPrefabs; // Masukkan prefab PetiSword, PetiHandCannon, PetiSqueekHammer ke array ini

    [Header("Titik Muncul Peti (Kosongkan jika ingin di tengah ruangan)")]
    public Transform chestSpawnPoint;

    // Fungsi yang akan dipanggil oleh DungeonManager saat room clear
    public void SpawnRandomChest()
{
    if (chestPrefabs == null || chestPrefabs.Length == 0) return;

    // Jarak pergeseran antar peti agar tidak saling bertumpukan di satu titik
    float spacing = 3.5f; 

    // Loop sebanyak jumlah prefab peti yang ada di array (yaitu 3 peti)
    for (int i = 0; i < chestPrefabs.Length; i++)
    {
        if (chestPrefabs[i] == null) continue;

        // Beri offset posisi X atau Z agar ketiga peti berjejer rapi
        Vector3 spawnPosition = chestSpawnPoint != null ? chestSpawnPoint.position : transform.position;
        spawnPosition.x += (i - 1) * spacing; // Mengatur posisi kiri, tengah, dan kanan
        spawnPosition.y += 0.2f;

        GameObject spawnedChest = Instantiate(chestPrefabs[i], spawnPosition, Quaternion.identity);
        spawnedChest.transform.parent = this.transform;
    }
}
}