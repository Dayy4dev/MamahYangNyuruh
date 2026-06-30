using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI atkBuffText;
    [SerializeField] private TextMeshProUGUI maxHpBuffText; 

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject; 

    [Header("Buff Manager Reference")]
    [Tooltip("Drag GameObject yang memiliki komponen PlayerBuffManager di sini.")]
    [SerializeField] private PlayerBuffManager buffManager;

    private void Start()
    {
        if (buffManager == null)
        {
            Debug.LogWarning("[InventoryUI] Buff Manager belum di-assign di Inspector! Buff text tidak akan terupdate.");
        }

        if (GameManager.Instance != null)
        {
            EvaluateVisibility(GameManager.Instance.CurrentState);
        }
        else
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Jika inventoryPanel sedang aktif di layar, terus perbarui teks buff-nya secara real-time
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            UpdateBuffStatusText();
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

        bool isWindowOpen = state == GameState.Inventory;
        inventoryPanel.SetActive(isWindowOpen);

        if (isWindowOpen)
        {
            if (equipmentUI != null)
            {
                equipmentUI.RefreshUI();
            }
            UpdateBuffStatusText(); 
        }
    }

    private void UpdateBuffStatusText()
    {
        if (buffManager != null)
        {
            // Menghitung total nilai gabungan akumulatif (Dari Tombol J/H + Dari Permen)
            int currentAtkBonus = buffManager.GetDamageBuffStack() * 50;
            int currentHpBonus = buffManager.GetHpBuffStack() * 100;

            if (atkBuffText != null)
            {
                atkBuffText.text = currentAtkBonus > 0 
                    ? $"BUFF ATK: <color=green>+{currentAtkBonus}</color> (x{buffManager.GetDamageBuffStack()})" 
                    : "BUFF ATK: ";
            }

            // SINKRONISASI DI SINI: Menggunakan currentHpBonus agar tidak eror lagi
            if (currentHpBonus > 0)
            {
                if (maxHpBuffText != null)
                {
                    maxHpBuffText.text = $"BUFF MAX HP: <color=green>+{currentHpBonus}</color> (x{buffManager.GetHpBuffStack()})";
                }
            }
            else
            {
                if (maxHpBuffText != null) maxHpBuffText.text = "BUFF MAX HP: ";
            }
        }
        else
        {
            if (atkBuffText != null) atkBuffText.text = "BUFF ATK: ";
            if (maxHpBuffText != null) maxHpBuffText.text = "BUFF MAX HP: ";
        }
    }
}