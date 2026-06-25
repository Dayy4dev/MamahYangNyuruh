using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject uiPanel; // Drag Game Object Teks/Panel UI ke sini
    [SerializeField] private TMP_Text promptText; // Drag komponen Text ke sini

    void OnEnable()
    {
        if (playerInventory != null)
            playerInventory.onNearestPickupChanged.AddListener(UpdateUI);
    }

    void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.onNearestPickupChanged.RemoveListener(UpdateUI);
    }

    private void UpdateUI(WeaponPickup nearestPickup)
    {
        // FIX LOGIC: Cek apakah komponen dan GameObject-nya benar-benar ada (tidak null/destroyed)
        if (nearestPickup != null && nearestPickup.gameObject != null)
        {
            // Nyalakan UI global di layar player
            if (uiPanel != null) uiPanel.SetActive(true);
            
            if (promptText != null)
            {
                // TIPS Tambahan: Kamu bisa memunculkan nama senjatanya secara dinamis jika mau!
                if (nearestPickup.Data != null)
                {
                    promptText.text = $"[F] Pick Up {nearestPickup.Data.weaponName}";
                }
                else
                {
                    promptText.text = "[F] To Pick Up";
                }
            }

            // Opsional: Jika senjatanya punya UI lokal sendiri, nyalakan juga
            nearestPickup.TogglePrompt(true);
        }
        else
        {
            // Jika tidak ada senjata terdekat ATAU senjata baru saja dihancurkan (di-pickup)
            if (uiPanel != null) uiPanel.SetActive(false);
        }
    }
}