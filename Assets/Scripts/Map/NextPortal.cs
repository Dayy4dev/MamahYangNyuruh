using UnityEngine;

// Attach ke GameObject portal di dalam prefab Left, Right, Top room.
// Portal aktif otomatis setelah room clear, dipanggil oleh DungeonManager.

public class NextMapPortal : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject portalVisual;

    [Header("Player Detection")]
    public string playerTag = "Player";

    private bool isActive = false;

    void Start()
    {
        SetPortalActive(false);
    }

    public void Activate()
    {
        isActive = true;
        SetPortalActive(true);
        Debug.Log("[Portal] Portal ke map berikutnya aktif!");
    }

    void SetPortalActive(bool active)
    {
        if (portalVisual != null)
            portalVisual.SetActive(active);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = active;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag(playerTag)) return;

        isActive = false; // cegah trigger ganda
        Debug.Log("[Portal] Player masuk portal, pindah map!");
        DungeonManager.Instance?.GoToNextMap();
    }
}