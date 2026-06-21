using UnityEngine;

// Attach ke GameObject portal/pintu di dalam prefab Left, Right, Top room.
// Portal ini TIDAK aktif dari awal — baru aktif setelah room clear.
public class NextMapPortal : MonoBehaviour
{
    [Header("Visuals")]
    public GameObject portalVisual;   // Efek visual portal (particle, glow, dll)

    [Header("Player Detection")]
    public string playerTag = "Player";

    private bool isActive = false;

    void Start()
    {
        // Portal mati dari awal
        SetActive(false);
    }

    // Dipanggil oleh DungeonManager setelah room clear
    public void Activate()
    {
        isActive = true;
        SetActive(true);
        Debug.Log("[Portal] Portal ke map berikutnya aktif!");
    }

    void SetActive(bool active)
    {
        if (portalVisual != null)
            portalVisual.SetActive(active);

        // Aktifkan/nonaktifkan collider trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = active;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag(playerTag)) return;

        Debug.Log("[Portal] Player masuk portal, pindah map!");
        isActive = false; // Cegah trigger ganda
        DungeonManager.Instance?.GoToNextMap();
    }
}