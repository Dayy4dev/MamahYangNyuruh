using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private Camera cam;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (cam == null)
            cam = Camera.main;

        if (slider == null)
            Debug.LogWarning("[EnemyHealthBar] Slider tidak ditemukan!");
    }

    void LateUpdate()
    {
        // Billboard: selalu menghadap kamera
        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void SetMaxHealth(int max)
    {
        slider.maxValue = max;
        slider.value    = max;
    }

    public void SetHealth(int current)
    {
        slider.value = current;
    }
}