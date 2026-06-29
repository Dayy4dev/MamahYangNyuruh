using UnityEngine;

public class LootboxManager : MonoBehaviour
{
    public static LootboxManager Instance { get; private set; }

    public enum Rarity { Legendary, Normal, Rusty }

    // --- TEMPAT MENYIMPAN STATUS AKTIF YANG BISA DIBACA OLEH UI ---
    [HideInInspector] public string currentBuff = "None";
    [HideInInspector] public string currentDebuff = "None";

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

    // --- FUNGSI BARU: Menerima nama efek dari kotak permen dan mengubahnya jadi teks status ---
    public void SetCandyEffectStatus(string effectName)
    {
        switch (effectName)
        {
            case "Heal":
                currentBuff = "Full Restore Health";
                break;
            case "MaxHP":
                currentBuff = "Max HP Limit (+30)";
                break;
            case "BuffDamage":
                currentBuff = "Permanent DMG Buff (+10)";
                break;
            case "InstantDamage":
                currentDebuff = "Curse: -50% Current HP";
                break;
        }
    }

    // Fungsi bawaan gacha senjata kamu (tetap aman tidak berubah)
    public GachaOutput OpenWeaponBox(GameObject[] legendaryPool, GameObject[] normalPool, GameObject[] rustyPool, string weaponName, Vector3 spawnPosition, Transform roomParent)
    {
        int roll = Random.Range(1, 11);
        Rarity obtainedRarity;
        GameObject[] selectedPool = null;

        if (roll <= 2)
        {
            obtainedRarity = Rarity.Legendary;
            selectedPool = legendaryPool;
            currentBuff = "ATK Booster (+20%)";
            currentDebuff = "None";
        }
        else if (roll <= 5)
        {
            obtainedRarity = Rarity.Normal;
            selectedPool = normalPool;
            currentBuff = Random.Range(0, 2) == 0 ? "Speed Up (+10%)" : "None";
            currentDebuff = "None";
        }
        else
        {
            obtainedRarity = Rarity.Rusty;
            selectedPool = rustyPool;
            currentBuff = "None";
            currentDebuff = Random.Range(0, 2) == 0 ? "Slow Move (-15%)" : "Decreased Defense";
        }

        GameObject prefabToSpawn = null;
        if (selectedPool != null && selectedPool.Length > 0)
        {
            int randomWeaponIndex = Random.Range(0, selectedPool.Length);
            prefabToSpawn = selectedPool[randomWeaponIndex];
        }

        if (prefabToSpawn != null)
        {
            GameObject spawnedWeapon = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            if (roomParent != null) spawnedWeapon.transform.parent = roomParent;
            return new GachaOutput { spawnedObject = spawnedWeapon, finalRarity = obtainedRarity };
        }
        return default;
    }
}