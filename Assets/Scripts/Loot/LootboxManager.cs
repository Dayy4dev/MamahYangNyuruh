using UnityEngine;

public class LootboxManager : MonoBehaviour
{
    // Tambahkan baris Instance ini di paling atas kelas
    public static LootboxManager Instance { get; private set; }

    public enum Rarity { Legendary, Normal, Rusty }

    public struct GachaOutput
    {
        public GameObject spawnedObject;
        public Rarity finalRarity;
    }

    private void Awake()
    {
        // Setup singleton agar mudah diakses oleh peti hasil kloning/spawn
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GachaOutput OpenWeaponBox(GameObject legendaryPrefab, GameObject biasaPrefab, GameObject rustyPrefab, string weaponName, Vector3 spawnPosition)
    {
        int roll = Random.Range(1, 11);
        Rarity obtainedRarity;
        GameObject prefabToSpawn = null;

        if (roll <= 2) { obtainedRarity = Rarity.Legendary; prefabToSpawn = legendaryPrefab; }
        else if (roll <= 5) { obtainedRarity = Rarity.Normal; prefabToSpawn = biasaPrefab; }
        else { obtainedRarity = Rarity.Rusty; prefabToSpawn = rustyPrefab; }

        if (prefabToSpawn != null)
        {
            GameObject spawnedWeapon = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            GachaOutput output = new GachaOutput();
            output.spawnedObject = spawnedWeapon;
            output.finalRarity = obtainedRarity;
            return output;
        }
        
        Debug.LogError("Prefab senjata kosong!");
        return default;
    }
}