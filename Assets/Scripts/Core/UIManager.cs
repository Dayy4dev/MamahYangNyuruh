using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameplayHUD;   // Drag your In-Game HUD object here
    [SerializeField] private GameObject inventoryUI;   // Drag your Inventory UI object here

    private void OnEnable()
    {
        // Subscribe to the GameManager's state change event
        GameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        // Always unsubscribe to avoid memory leaks
        GameManager.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        // Initialize layout visibility based on current GameManager state on launch
        if (GameManager.Instance != null)
        {
            UpdateUIElements(GameManager.Instance.CurrentState);
        }
    }

    // This runs automatically whenever GameManager changes state (e.g. pressing I or Escape)
    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        UpdateUIElements(newState);
    }

    private void UpdateUIElements(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                if (gameplayHUD != null) gameplayHUD.SetActive(true);
                if (inventoryUI != null) inventoryUI.SetActive(false);
                break;

            case GameState.Inventory:
                if (gameplayHUD != null) gameplayHUD.SetActive(false);
                if (inventoryUI != null) inventoryUI.SetActive(true);
                break;

            case GameState.Paused:
                // Keep UI states clean if you pause from either menu
                if (inventoryUI != null) inventoryUI.SetActive(false);
                break;

            case GameState.GameOver:
                if (gameplayHUD != null) gameplayHUD.SetActive(false);
                if (inventoryUI != null) inventoryUI.SetActive(false);
                break;
        }
    }
}