using UnityEngine;

public class LootboxChestTrigger : MonoBehaviour
{
    [Header("Referensi Otak Gacha")]
    public LootboxManager lootboxManager;

    [Header("Rarity Hasil Gacha")]
    public LootboxManager.Rarity weaponRarity; 

    [Header("Titik Muncul Senjata")]
    public Transform mySpawnPoint; 

    [Header("Identitas Senjata Box Ini")]
    public string weaponName; 

    // SEKARANG MENGGUNAKAN ARRAY AGAR BISA RANDOMIZE ISI SENJATANYA
    [Header("Pool Varian Model 3D (Isi Banyak Lebih Seru!)")]
    public GameObject[] legendaryPrefabs;
    public GameObject[] biasaPrefabs;
    public GameObject[] rustyPrefabs;

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
        if (lootboxManager == null)
        {
            lootboxManager = LootboxManager.Instance;
        }

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

            Transform currentRoomParent = this.transform.parent;

            // Mengirimkan array pool senjata ke manager untuk diacak isinya
            LootboxManager.GachaOutput result = lootboxManager.OpenWeaponBox(
                legendaryPrefabs, biasaPrefabs, rustyPrefabs, weaponName, mySpawnPoint.position, currentRoomParent
            );
            
            mySpawnedWeapon = result.spawnedObject;
            weaponRarity = result.finalRarity; 
            
            if (mySpawnedWeapon != null)
            {
                WeaponPickup pickupComponent = mySpawnedWeapon.GetComponent<WeaponPickup>();
                if (pickupComponent != null)
                {
                    pickupComponent.customRarity = weaponRarity.ToString();
                }
            }
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