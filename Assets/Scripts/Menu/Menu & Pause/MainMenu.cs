using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private MenuState currentState = MenuState.Playing;
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public GameObject pauseMenu;

    [Header("UI Tambahan")]
    // Tarik GameObject 'PlaySelectionMenu' ke slot ini di Inspector
    public GameObject playSelectionMenu; 
    public GameObject mainMenuPanel;

    private void Start()
    {
        // Paksa menyala di awal agar tidak ter-hide otomatis
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        
        // Paksa mati di awal untuk pilihan tutorialnya
        if (playSelectionMenu != null)
        {
            playSelectionMenu.SetActive(false); 
        }

        LoadVolume();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Jika menu pilihan sedang terbuka, tekan Esc akan menutupnya
            if (playSelectionMenu != null && playSelectionMenu.activeSelf)
            {
                ClosePlaySelection();
            }
            else if (currentState == MenuState.Playing)
            {
                PauseGame();
            }
            else if (currentState == MenuState.Paused)
            {
                ResumeGame();
            }
        }
    }

    public void BackMenu()
    {
        LevelManager.Instance.LoadScene("MainMenu", "CircleWipe");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // MODIFIKASI: Sekarang fungsi Play hanya membuka UI pilihan
    public void Play()
    {
      
        if (playSelectionMenu != null)
        {
            playSelectionMenu.SetActive(true);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false); // <--- Mematikan menu utama
        }
    }
    

    // Fungsi untuk menutup UI pilihan jika klik tombol 'Back'
    public void ClosePlaySelection()
    {
        if (playSelectionMenu != null)
        {
            playSelectionMenu.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true); // <--- Menyalakan kembali menu utama
        }
    }

    // PILIHAN 1: Mulai dari Tutorial Scene
    public void StartTutorial()
    {
        MusicManager.Instance.StopLobbyPlaylist();
        LevelManager.Instance.LoadScene("Tutorial", "CircleWipe"); 
        MusicManager.Instance.PlayMusic("Game");

        Time.timeScale = 1f;
        currentState = MenuState.Playing;
    }

    // PILIHAN 2: Langsung Main (Ganti "PlayScene" dengan nama scene gameplay utama Anda)
    public void StartDirectGame()
    {
        MusicManager.Instance.StopLobbyPlaylist();
        LevelManager.Instance.LoadScene("PlayScene", "CircleWipe"); 
        MusicManager.Instance.PlayMusic("Game");

        Time.timeScale = 1f;
        currentState = MenuState.Playing;
    }

    public void PauseGame()
    {
        currentState = MenuState.Paused;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }

        if (currentState == MenuState.Paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ResumeGame()
    {
        currentState = MenuState.Playing;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
    }

    public void ShowCursorSettings()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;                  
    }
}