using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

    // --- SLOT BARU UNTUK MEMASUKKAN OBJEK TEKS DI INSPECTOR ---
    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI atkBuffText;
    [SerializeField] private TextMeshProUGUI maxHpBuffText;

    // Menampung total nilai buff yang sudah diakumulasikan
    private int totalAtkBuff = 0;
    private int totalMaxHpBuff = 0;

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

            // AMBIL DATA TERBARU DAN UPDATE TEKS STATUS BUFF ATK & MAX HP
            RefreshBuffText();
        }
    }

    private void RefreshBuffText()
    {
        if (atkBuffText == null || maxHpBuffText == null) return;

        // Tarik data dari pusat data di LootboxManager
        if (LootboxManager.Instance != null)
        {
            // --- CATATAN PENTING ---
            // Di sini diasumsikan LootboxManager.Instance.currentBuff mengembalikan nilai angka dalam bentuk int atau float (misal: 10).
            // Jika currentBuff saat ini masih berbentuk string (misal: "+10 ATK"), kamu perlu mengubah tipe datanya di LootboxManager menjadi angka (int).
            
            int newAtkBuff = LootboxManager.Instance.currentAtkBuff; // Ganti dengan variabel ATK baru dari LootboxManager-mu
            int newMaxHpBuff = LootboxManager.Instance.currentMaxHpBuff; // Ganti dengan variabel Max HP baru dari LootboxManager-mu

            // Akumulasikan nilai lama dengan nilai baru yang didapat
            totalAtkBuff += newAtkBuff;
            totalMaxHpBuff += newMaxHpBuff;

            // Cetak teks ke masing-masing TextMeshPro UI dengan warna hijau
            atkBuffText.text = totalAtkBuff > 0 ? $"BUFF ATK: <color=green>+{totalAtkBuff}</color>" : "BUFF ATK: 0";
            maxHpBuffText.text = totalMaxHpBuff > 0 ? $"BUFF MAX HP: <color=green>+{totalMaxHpBuff}</color>" : "BUFF MAX HP: 0";

            // Setelah nilai ditambahkan ke total, reset buff di LootboxManager agar tidak terus bertambah secara tidak sengaja di frame berikutnya
            LootboxManager.Instance.ResetCurrentBuffs(); 
        }
        else
        {
            atkBuffText.text = "BUFF ATK: 0";
            maxHpBuffText.text = "BUFF MAX HP: 0";
        }
    }
}