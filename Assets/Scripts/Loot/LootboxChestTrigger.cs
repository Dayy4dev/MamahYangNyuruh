using UnityEngine;

public class LootboxChestTrigger : MonoBehaviour
{
    [Header("Referensi Otak Gacha")]
    public LootboxManager lootboxManager;

    [Header("Identitas Senjata Box Ini")]
    public string weaponName; 

    [Header("Varian Model 3D Box Ini")]
    public GameObject legendaryPrefab;
    public GameObject biasaPrefab;
    public GameObject rustyPrefab;

    private bool isPlayerNearby = false;

    private void Update()
    {
        // Jika player berada di dekat peti DAN menekan tombol E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (lootboxManager != null)
        {
            lootboxManager.OpenWeaponBox(legendaryPrefab, biasaPrefab, rustyPrefab, weaponName);
            
            // Opsional: Nonaktifkan peti ini setelah dibuka agar tidak bisa dispam
            // this.enabled = false; 
        }
    }

    // Mendeteksi jika Player (yang memiliki Collider & Rigidbody) mendekati Peti
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Pastikan Player kamu di Inspector memiliki Tag "Player"
        {
            isPlayerNearby = true;
            Debug.Log("Tekan 'F' untuk membuka peti!");
        }
    }

    // Mendeteksi jika Player menjauh dari Peti
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}