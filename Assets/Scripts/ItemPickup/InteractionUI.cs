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
        if (nearestPickup != null)
        {
            // Nyalakan UI dan ubah teksnya sesuai nama senjata
            uiPanel.SetActive(true);
            if (promptText != null)
            {
                promptText.text = $"[F] To Pick Up";
            }

            // Opsional: Jika senjatanya punya UI lokal sendiri, nyalakan juga
            nearestPickup.TogglePrompt(true);
        }
        else
        {
            // Jika tidak ada senjata terdekat, matikan UI
            uiPanel.SetActive(false);
        }
    }
}
