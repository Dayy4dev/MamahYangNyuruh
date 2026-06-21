using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel Context References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

    private void Awake()
    {
        if (inventoryPanel == null)
        {
            inventoryPanel = this.gameObject;
        }
    }

    private void Start()
    {
        // Force the panel to look at the manager's state on frame one
        if (GameManager.Instance != null)
        {
            UpdateVisibility(GameManager.Instance.CurrentState);
        }
        else
        {
            inventoryPanel.SetActive(false); // Fallback safe closure
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
        UpdateVisibility(newState);
    }

    private void UpdateVisibility(GameState state)
    {
        if (inventoryPanel == null) return;

        bool shouldBeOpen = (state == GameState.Inventory);
        inventoryPanel.SetActive(shouldBeOpen);

        if (shouldBeOpen && equipmentUI != null)
        {
            equipmentUI.Refresh();
        }
    }
}