using UnityEngine;

public class LootboxManager : MonoBehaviour
{
    public enum WeaponTier { Legendary, Biasa, Rusty }

    [Header("3D Spawn Settings")]
    public Transform spawnPoint;
    public float rotationSpeed = 70f;
    public float hoverSpeed = 3f;
    public float hoverAmount = 0.15f;

    private GameObject currentSpawnedWeapon;
    private Vector3 startSpawnPos;

    // Fungsi ini yang akan dipanggil oleh LootboxButton kamu
    public void OpenWeaponBox(GameObject legendaryPrefab, GameObject biasaPrefab, GameObject rustyPrefab, string weaponName)
    {
        // 1. Hapus senjata yang melayang sebelumnya jika ada
        if (currentSpawnedWeapon != null) 
            Destroy(currentSpawnedWeapon);

        // 2. Roll Angka Acak 1 - 10 (Rate 2 : 3 : 5)
        int roll = Random.Range(1, 11);
        WeaponTier obtainedTier;
        GameObject prefabToSpawn = null;

        if (roll <= 2)
        {
            obtainedTier = WeaponTier.Legendary;
            prefabToSpawn = legendaryPrefab;
        }
        else if (roll <= 5)
        {
            obtainedTier = WeaponTier.Biasa;
            prefabToSpawn = biasaPrefab;
        }
        else
        {
            obtainedTier = WeaponTier.Rusty;
            prefabToSpawn = rustyPrefab;
        }

        // 3. Spawn Model 3D di Game World
        if (prefabToSpawn != null && spawnPoint != null)
        {
            currentSpawnedWeapon = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
            startSpawnPos = spawnPoint.position;
            
            Debug.Log($"<color=green>🎉 BERHASIL GACHA!</color> Kamu mendapatkan <b>[{obtainedTier}] {weaponName}</b>!");
        }
        else
        {
            Debug.LogError("Prefab senjata kosong! Pastikan sudah memasukkan prefab di tombol UI.");
        }
    }

    private void Update()
    {
        // Efek visual ala game Isometric: Senjata berputar dan melayang naik-turun
        if (currentSpawnedWeapon != null)
        {
            // Berputar otomatis
            currentSpawnedWeapon.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
            
            // Melayang naik turun menggunakan sin (Trigonometri)
            float newY = startSpawnPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
            currentSpawnedWeapon.transform.position = new Vector3(currentSpawnedWeapon.transform.position.x, newY, currentSpawnedWeapon.transform.position.z);
        }
    }
}