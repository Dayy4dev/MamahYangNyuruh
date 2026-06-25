using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

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

        // Refresh the graphics data right when the menu pops up
        if (isWindowOpen && equipmentUI != null)
        {
            equipmentUI.RefreshUI();
        }
    }
}