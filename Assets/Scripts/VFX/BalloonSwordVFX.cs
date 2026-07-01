using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class BalloonSwordVFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject slashVfxPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 1f);
    [SerializeField] private Vector3 localRotation = new Vector3(0, 0, 0);
    
    [Header("Timing Settings")]
    [SerializeField] private float playDelay = 0f;
    
    private GameObject currentVfxInstance;
    private VideoPlayer videoPlayer;

    private void Start()
    {
        if (slashVfxPrefab != null)
        {
            currentVfxInstance = Instantiate(slashVfxPrefab, transform);
            currentVfxInstance.transform.localPosition = offset;
            currentVfxInstance.transform.localEulerAngles = localRotation;
            currentVfxInstance.SetActive(false);

            videoPlayer = currentVfxInstance.GetComponentInChildren<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnVideoEnd;
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
            }
            else
            {
                Debug.LogWarning("[BalloonSwordVFX] No VideoPlayer found in the spawned VFX prefab.");
            }
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }

    public void PlayVFX()
    {
        if (currentVfxInstance != null && videoPlayer != null)
        {
            StopAllCoroutines();
            
            if (playDelay > 0f)
            {
                StartCoroutine(PlayVFXWithDelay());
            }
            else
            {
                ExecutePlayVFX();
            }
        }
    }

    private IEnumerator PlayVFXWithDelay()
    {
        yield return new WaitForSeconds(playDelay);
        ExecutePlayVFX();
    }

    private void ExecutePlayVFX()
    {
        if (currentVfxInstance == null || videoPlayer == null) return;

        currentVfxInstance.transform.localPosition = offset;
        currentVfxInstance.transform.localEulerAngles = localRotation;

        currentVfxInstance.SetActive(true);
        
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public void StopVFX()
    {
        StopAllCoroutines();
        
        if (currentVfxInstance != null)
        {
            currentVfxInstance.SetActive(false);
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (currentVfxInstance != null)
        {
            currentVfxInstance.SetActive(false);
        }
    }
}