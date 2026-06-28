using UnityEngine;

public class CandyBox : MonoBehaviour
{
    private string myCandyEffect;
    private PartnerSystem mainPartnerSystem;
    private bool hasBeenOpened = false;
    
    // Variabel baru untuk mengecek apakah player sedang berada di dekat kotak
    private bool isPlayerNearby = false;

    public void InitializeBox(string effect, PartnerSystem partner)
    {
        myCandyEffect = effect;
        mainPartnerSystem = partner;
        hasBeenOpened = false;
        isPlayerNearby = false;
    }

    // Setiap frame, Unity akan mengecek apakah player menekan tombol interaksi
    void Update()
    {
        if (isPlayerNearby && !hasBeenOpened)
        {
            // Kamu bisa mengganti KeyCode.E dengan tombol lain sesuai sistem inputmu
            if (Input.GetKeyDown(KeyCode.F))
            {
                OpenBox();
            }
        }
    }

    public void OpenBox()
    {
        if (hasBeenOpened) return;
        hasBeenOpened = true;

        Debug.Log($"[Box Interaction] Player memilih kotak ini! Isi permen: {myCandyEffect}");
        mainPartnerSystem.ProcessBoxSelection(myCandyEffect);
    }

    public void RevealAndCleanUp()
    {
        hasBeenOpened = true;
        Debug.Log($"[Box Reveal] Kotak sisa terbuka otomatis: {myCandyEffect}");
        Destroy(mainPartnerSystem.gameObject, 2.5f);
    }

    // Deteksi saat Player masuk ke area kotak
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // OPSIONAL: Kamu bisa memunculkan UI text "Tekan E untuk memilih" di sini jika ada
            Debug.Log("Dekat kotak permen. Tekan 'E' untuk memilih!");
        }
    }

    // Deteksi saat Player pergi menjauh dari area kotak
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}