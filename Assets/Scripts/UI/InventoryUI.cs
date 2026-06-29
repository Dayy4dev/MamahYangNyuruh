using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

    // --- SLOT BARU UNTUK MEMASUKKAN OBJEK TEKS DI INSPECTOR ---
    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI buffDebuffText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            EvaluateVisibility(GameManager.Instance.CurrentState);
        }
        else
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        EvaluateVisibility(newState);
    }

    private void EvaluateVisibility(GameState state)
    {
        if (inventoryPanel == null) return;

        bool isWindowOpen = (state == GameState.Inventory);
        inventoryPanel.SetActive(isWindowOpen);

        // Ketika tombol E ditekan dan inventory terbuka, refresh semua teks UI
        if (isWindowOpen)
        {
            if (equipmentUI != null)
            {
                equipmentUI.RefreshUI();
            }

            // AMBIL DATA TERBARU DAN UPDATE TEKS STATUS BUFF/DEBUFF
            RefreshBuffDebuffText();
        }
    }

    private void RefreshBuffDebuffText()
    {
        if (buffDebuffText == null) return;

        // Tarik data dari pusat data di LootboxManager
        if (LootboxManager.Instance != null)
        {
            string activeBuff = LootboxManager.Instance.currentBuff;
            string activeDebuff = LootboxManager.Instance.currentDebuff;

            // Jika statusnya bukan "None", beri tag warna Richtext (<color=warna>)
            string buffFormat = activeBuff != "None" ? $"<color=green>{activeBuff}</color>" : "None";
            string debuffFormat = activeDebuff != "None" ? $"<color=red>{activeDebuff}</color>" : "None";

            // Cetak gabungan teks ke TextMeshPro UI
            buffDebuffText.text = $"BUFF: {buffFormat}\nDEBUFF: {debuffFormat}";
        }
        else
        {
            buffDebuffText.text = "BUFF: None\nDEBUFF: None";
        }
    }
}