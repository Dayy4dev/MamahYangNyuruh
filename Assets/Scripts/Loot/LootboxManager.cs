using UnityEngine;

public class LootboxManager : MonoBehaviour
{
    public static LootboxManager Instance { get; private set; }

    public enum Rarity { Legendary, Normal, Rusty }

    public struct GachaOutput
    {
        public GameObject spawnedObject;
        public Rarity finalRarity;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // SEKARANG MENERIMA ARRAY (GameObject[]) UNTUK MASING-MASING TIER
    public GachaOutput OpenWeaponBox(GameObject[] legendaryPool, GameObject[] normalPool, GameObject[] rustyPool, string weaponName, Vector3 spawnPosition, Transform roomParent)
    {
        // 1. Roll Angka Acak 1 - 10 (Rate 2 : 3 : 5)
        int roll = Random.Range(1, 11);
        Rarity obtainedRarity;
        GameObject[] selectedPool = null;

        if (roll <= 2)
        {
            obtainedRarity = Rarity.Legendary;
            selectedPool = legendaryPool;
        }
        else if (roll <= 5)
        {
            obtainedRarity = Rarity.Normal;
            selectedPool = normalPool;
        }
        else
        {
            obtainedRarity = Rarity.Rusty;
            selectedPool = rustyPool;
        }

        // 2. ACAK SENJATA DARI POOL YANG TERPILIH
        GameObject prefabToSpawn = null;
        if (selectedPool != null && selectedPool.Length > 0)
        {
            int randomWeaponIndex = Random.Range(0, selectedPool.Length);
            prefabToSpawn = selectedPool[randomWeaponIndex];
        }

        // 3. Spawn Senjata ke game
        if (prefabToSpawn != null)
        {
            GameObject spawnedWeapon = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            if (roomParent != null)
            {
                spawnedWeapon.transform.parent = roomParent;
            }

            Debug.Log($"<color=lime>🎉 [Lootbox]</color> Membuka Peti! Mendapatkan Senjata Acak: <b>{prefabToSpawn.name}</b> dengan Rarity: <b>[{obtainedRarity}]</b>");
            
            GachaOutput output = new GachaOutput();
            output.spawnedObject = spawnedWeapon;
            output.finalRarity = obtainedRarity;

            return output;
        }
        else
        {
            Debug.LogError("Gagal gacha! Pool senjata kosong atau prefab di dalam array ada yang null.");
            return default;
        }
    }
}