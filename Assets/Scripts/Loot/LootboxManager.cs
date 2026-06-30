using UnityEngine;

public class LootboxManager : MonoBehaviour
{
    public static LootboxManager Instance { get; private set; }

    public enum Rarity { Legendary, Normal, Rusty }

    // --- TEMPAT MENYIMPAN BUFFER NILAI BUFF BARU YANG DIDAPAT ---
    // Variabel ini menampung buff yang baru didapat dari peti/permen saat ini
    [HideInInspector] public int currentAtkBuff = 0;
    [HideInInspector] public int currentMaxHpBuff = 0;

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

    // --- FUNGSI UPDATE: Menambah buff angka berdasarkan efek permen ---
    public void SetCandyEffectStatus(string effectName)
    {
        switch (effectName)
        {
            case "Heal":
                // Jika ini adalah restore health biasa (bukan nambah batas Max HP), 
                // kodenya bisa ditaruh di sini atau dibiarkan jika diurus script lain.
                break;
            case "MaxHP":
                currentMaxHpBuff = 30; // Mengirim angka 30 ke UI
                break;
            case "BuffDamage":
                currentAtkBuff = 10; // Mengirim angka 10 ke UI
                break;
            case "InstantDamage":
                // Debuff dihapus sesuai permintaan
                break;
        }
    }

    // Fungsi bawaan gacha senjata kamu (Sudah disesuaikan tanpa Debuff & Menggunakan Angka)
    public GachaOutput OpenWeaponBox(GameObject[] legendaryPool, GameObject[] normalPool, GameObject[] rustyPool, string weaponName, Vector3 spawnPosition, Transform roomParent)
    {
        int roll = Random.Range(1, 11);
        Rarity obtainedRarity;
        GameObject[] selectedPool = null;

        // Reset buffer terlebih dahulu sebelum diisi hasil gacha baru
        currentAtkBuff = 0;
        currentMaxHpBuff = 0;

        if (roll <= 2)
        {
            obtainedRarity = Rarity.Legendary;
            selectedPool = legendaryPool;
            currentAtkBuff = 20; // ATK Booster diganti jadi angka +20
        }
        else if (roll <= 5)
        {
            obtainedRarity = Rarity.Normal;
            selectedPool = normalPool;
            // Buff speed disesuaikan atau dihilangkan karena fokus ke ATK & Max HP
            currentAtkBuff = 0; 
        }
        else
        {
            obtainedRarity = Rarity.Rusty;
            selectedPool = rustyPool;
            // Debuff di peti Rusty dihapus otomatis
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

    // --- FUNGSI BARU: Dipanggil oleh InventoryUI setelah berhasil mengambil nilainya ---
    public void ResetCurrentBuffs()
    {
        currentAtkBuff = 0;
        currentMaxHpBuff = 0;
    }
}