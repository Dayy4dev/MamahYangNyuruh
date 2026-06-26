using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;
    [SerializeField]
    private List<string> lobbyPlaylist = new List<string> { "Lobby Sound 1", "Lobby Sound 2" };
    private Coroutine playlistCoroutine;
    private string currentTrackName = "";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            StopLobbyPlaylist();

            currentTrackName = "";

            if (musicSource != null) musicSource.loop = false;

            playlistCoroutine = StartCoroutine(PlayLobbyPlaylistAutomatically());
        }
        // TAMBAHKAN BLOK ELSE IF INI (Sesuaikan "GameScene" dengan nama scene permainanmu)
        else if (scene.name == "GameScene")
        {
            // Pastikan playlist lobby dimatikan total saat masuk atau reload game scene
            StopLobbyPlaylist();

            // Mainkan kembali lagu in-game agar saat respawn tidak balik ke lagu lobby
            PlayMusic("Game");
        }
    }
    private void Start()
    {
        playlistCoroutine = StartCoroutine(PlayLobbyPlaylistAutomatically());
    }
    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        if (trackName == currentTrackName) return;
        currentTrackName = trackName;

        if (musicSource != null)
        {
            musicSource.loop = true;
        }

        StartCoroutine(AnimateMusicCrossfade(musicLibrary.GetClipFromName(trackName), fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }

    IEnumerator PlayLobbyPlaylistAutomatically()
    {
        int currentIndex = 0;

        while (true)
        {
            string trackToPlay = lobbyPlaylist[currentIndex];
            AudioClip nextClip = musicLibrary.GetClipFromName(trackToPlay);

            if (nextClip != null)
            {
                currentTrackName = trackToPlay;
                yield return StartCoroutine(AnimateMusicCrossfade(nextClip, 0.5f));
            }

            yield return new WaitForSeconds(0.2f);

            while (musicSource.isPlaying)
            {
                yield return null;
            }

            currentIndex++;

            if (currentIndex >= lobbyPlaylist.Count)
            {
                currentIndex = 0;
            }
        }
    }

    public void StopLobbyPlaylist()
    {
        if (playlistCoroutine != null)
        {
            StopCoroutine(playlistCoroutine);
        }
    }
}