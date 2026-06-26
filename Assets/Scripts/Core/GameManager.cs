using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<GameState, GameState> OnStateChanged;

    public GameState CurrentState { get; private set; } = GameState.Playing; // Force initial tracking assignment
    private GameState previousState = GameState.Playing;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private KeyCode inventoryKey = KeyCode.E;

    public bool IsPlaying => CurrentState == GameState.Playing;
    public bool IsPaused => CurrentState == GameState.Paused;
    public bool IsInventoryOpen => CurrentState == GameState.Inventory;
    public bool IsMenuOpen => CurrentState == GameState.Paused || CurrentState == GameState.Inventory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Enforce clean layout start settings explicitly
        CurrentState = GameState.Playing;
        ApplyStateEffects();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (CurrentState == GameState.GameOver) return;

        // Use GetKeyDown to guarantee it only triggers once per physical finger click
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
    }

    public void TogglePause()
    {
        switch (CurrentState)
        {
            case GameState.Playing:
            case GameState.Inventory:
                previousState = CurrentState;
                SetState(GameState.Paused);
                break;

            case GameState.Paused:
                // Return cleanly to whatever screen we were looking at prior to pausing
                SetState(previousState);
                break;
        }
    }

    public void ToggleInventory()
    {
        switch (CurrentState)
        {
            case GameState.Playing:
                SetState(GameState.Inventory);
                break;

            case GameState.Inventory:
                SetState(GameState.Playing);
                break;

            case GameState.Paused:
                // Do nothing if the pause panel is open to prevent overlapping state conflicts
                break;
        }
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        GameState oldState = CurrentState;
        CurrentState = newState;

        ApplyStateEffects();
        Debug.Log($"State Changed: {oldState} -> {newState}");
        OnStateChanged?.Invoke(oldState, newState);
    }

    private void ApplyStateEffects()
    {
        // Unfreeze time scale explicitly when playing, freeze it when navigating UI menus
        Time.timeScale = (CurrentState == GameState.Playing) ? 1f : 0f;

        if (CurrentState == GameState.GameOver)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void GameOver()
    {
        SetState(GameState.GameOver);
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetState(previousState);
        }
    }
    public void RespawnGame()
    {
        Time.timeScale = 1f; // Kembalikan waktu jadi normal

        string playedScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene(playedScene, "CircleWipe");
        }
        else
        {
            // Cadangan aman jika LevelManager tidak sengaja tidak ada di scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(playedScene);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Kembalikan waktu jadi normal
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadScene("MainMenu", "CircleWipe");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}