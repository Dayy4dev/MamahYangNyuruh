using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI atkBuffText;
    [SerializeField] private TextMeshProUGUI maxHpBuffText; // Khusus teks tulisan "BUFF MAX HP: X"
    


    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject; 

    [Header("Buff Manager Reference")]
    [Tooltip("Drag GameObject yang memiliki komponen PlayerBuffManager di sini (misal: Game Manager atau Player, sesuai di mana komponen itu sebenarnya berada).")]
    [SerializeField] private PlayerBuffManager buffManager;

    private void Start()
    {
        // CATATAN: buffManager sekarang di-assign langsung lewat Inspector (field di atas),
        // tidak lagi dicari otomatis via playerObject.GetComponentInChildren(),
        // karena PlayerBuffManager bisa saja berada di GameObject lain (misal Game Manager).
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
            int currentAtkBonus = buffManager.GetDamageBuffStack() * 50;
            int currentHpBonus = buffManager.GetHpBuffStack() * 100;

            if (atkBuffText != null)
            {
                atkBuffText.text = currentAtkBonus > 0 
                    ? $"BUFF ATK: <color=green>+{currentAtkBonus}</color> (x{buffManager.GetDamageBuffStack()})" 
                    : "BUFF ATK: ";
            }

            if (maxHpBuffText != null)
            {
                maxHpBuffText.text = currentHpBonus > 0 
                    ? $"BUFF MAX HP: <color=green>+{currentHpBonus}</color> (x{buffManager.GetHpBuffStack()})" 
                    : "BUFF MAX HP: ";
            }
        }
        else
        {
            if (atkBuffText != null) atkBuffText.text = "BUFF ATK: ";
            if (maxHpBuffText != null) maxHpBuffText.text = "BUFF MAX HP: ";
        }
    }


}