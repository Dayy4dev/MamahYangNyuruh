using UnityEngine;

public class LootboxChestTrigger : MonoBehaviour
{
    [Header("Referensi Otak Gacha (Akan dicari otomatis saat Start)")]
    public LootboxManager lootboxManager;

    [Header("Rarity Hasil Gacha")]
   public LootboxManager.Rarity weaponRarity;

    [Header("Titik Muncul Senjata")]
    public Transform mySpawnPoint; 

    [Header("Identitas Senjata Box Ini")]
    public string weaponName; 

    [Header("Varian Model 3D Box Ini")]
    public GameObject legendaryPrefab;
    public GameObject biasaPrefab;
    public GameObject rustyPrefab;

    [Header("Visual Animation Settings")]
    public float rotationSpeed = 70f;
    public float hoverSpeed = 3f;
    public float hoverAmount = 0.15f;

    [Header("Tombol Interaksi")]
    public KeyCode interactionKey = KeyCode.E;

    private bool isPlayerNearby = false;
    private bool isOpened = false; 
    private GameObject mySpawnedWeapon; 
    private Vector3 startSpawnPos;

    private void Start()
    {
        // KUNCI MANDIRI: Cari otomatis LootboxManager di scene biar tidak error saat diclone
        if (lootboxManager == null)
        {
            lootboxManager = LootboxManager.Instance;
        }

        // Pengaman jika kamu lupa membuat objek kosong anak sebagai titik spawn
        if (mySpawnPoint == null)
        {
            mySpawnPoint = transform.Find("SpawnPoint"); 
            if (mySpawnPoint == null) mySpawnPoint = this.transform; 
        }
    }

    private void Update()
    {
        if (isPlayerNearby && !isOpened && Input.GetKeyDown(interactionKey))
        {
            OpenChest();
        }

        if (mySpawnedWeapon != null)
        {
            mySpawnedWeapon.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
            float newY = startSpawnPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
            mySpawnedWeapon.transform.position = new Vector3(mySpawnedWeapon.transform.position.x, newY, mySpawnedWeapon.transform.position.z);
        }
    }

    private void OpenChest()
    {
        if (lootboxManager != null && mySpawnPoint != null)
        {
            isOpened = true; 
            startSpawnPos = mySpawnPoint.position;

            LootboxManager.GachaOutput result = lootboxManager.OpenWeaponBox(legendaryPrefab, biasaPrefab, rustyPrefab, weaponName, mySpawnPoint.position);
            
            mySpawnedWeapon = result.spawnedObject;
            weaponRarity = result.finalRarity; 
        }
        else
        {
            Debug.LogError($"LootboxManager atau MySpawnPoint belum di-assign pada {gameObject.name}!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }
}