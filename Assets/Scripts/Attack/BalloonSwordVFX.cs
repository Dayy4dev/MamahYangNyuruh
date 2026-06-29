using UnityEngine;
using UnityEngine.Video;

public class BalloonSwordVFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject slashVfxPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 1f);
    
    private GameObject currentVfxInstance;
    private VideoPlayer videoPlayer;

    private void Start()
    {
        if (slashVfxPrefab != null)
        {
            // Instantiate the VFX prefab and parent it to this weapon
            currentVfxInstance = Instantiate(slashVfxPrefab, transform);
            currentVfxInstance.transform.localPosition = offset;
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
            // Reset position and rotation
            currentVfxInstance.transform.localPosition = offset;
            currentVfxInstance.transform.localRotation = Quaternion.identity;

            currentVfxInstance.SetActive(true);
            
            // Fast forward to beginning and play
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    public void StopVFX()
    {
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
        // Automatically hide the VFX when the video finishes
        if (currentVfxInstance != null)
        {
            currentVfxInstance.SetActive(false);
        }
    }
}
