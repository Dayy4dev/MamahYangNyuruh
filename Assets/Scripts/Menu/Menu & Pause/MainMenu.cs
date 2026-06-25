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

    private void Start()
    {
        LoadVolume();
        // MusicManager.Instance.PlayMusic("Lobby Sound 1");;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == MenuState.Playing)
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
    }

    public void Play()
    {
        MusicManager.Instance.StopLobbyPlaylist();
        LevelManager.Instance.LoadScene("GameScene", "CircleWipe");
        MusicManager.Instance.PlayMusic("Game");

        Time.timeScale = 1f;
        currentState = MenuState.Playing;
    }

    public void PauseGame()
    {
        currentState = MenuState.Paused;
        Time.timeScale = 0f;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        currentState = MenuState.Playing;
        Time.timeScale = 1f;
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
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
}
