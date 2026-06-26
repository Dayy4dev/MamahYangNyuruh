using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject gameplayUI;   // Drag your 'Gameplay Health' here
    [SerializeField] private GameObject inventoryUI;  // Drag your 'Inventory UI' here
    [SerializeField] private GameObject pauseUI;      // Drag your 'Pause Panel' here
    [SerializeField] private GameObject gameOverUI;

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
                if (gameplayUI != null) gameplayUI.SetActive(true);
                if (inventoryUI != null) inventoryUI.SetActive(false);
                if (pauseUI != null) pauseUI.SetActive(false);
                break;

            case GameState.Inventory:
                if (gameplayUI != null) gameplayUI.SetActive(false);
                if (inventoryUI != null) inventoryUI.SetActive(true);
                if (pauseUI != null) pauseUI.SetActive(false);
                break;

            case GameState.Paused:
                // Hide gameplay and inventory, show the pause screen
                if (gameplayUI != null) gameplayUI.SetActive(false);
                if (inventoryUI != null) inventoryUI.SetActive(false);
                if (pauseUI != null) pauseUI.SetActive(true);
                break;

            case GameState.GameOver:
                if (gameplayUI != null) gameplayUI.SetActive(false);
                if (inventoryUI != null) inventoryUI.SetActive(false);
                if (pauseUI != null) pauseUI.SetActive(false);
                if (gameOverUI != null) gameOverUI.SetActive(true);
                break;
        }
    }
}