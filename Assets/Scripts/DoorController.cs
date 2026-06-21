using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public RoomType destinationRoomType;
    public bool isLocked = true;

    [Header("Visuals (Opsional)")]
    public GameObject lockedVisual;
    public GameObject unlockedVisual;

    [Header("Player Detection")]
    public string playerTag = "Player";

    void Start()
    {
        RefreshVisual();
    }

    public void LockDoor()
    {
        isLocked = true;
        RefreshVisual();
    }

    public void UnlockDoor()
    {
        isLocked = false;
        RefreshVisual();
        Debug.Log($"[Door] Pintu ke {destinationRoomType} terbuka!");
    }

    void RefreshVisual()
    {
        if (lockedVisual != null) lockedVisual.SetActive(isLocked);
        if (unlockedVisual != null) unlockedVisual.SetActive(!isLocked);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLocked && other.CompareTag(playerTag))
            EnterDoor(other.gameObject);
    }

    void EnterDoor(GameObject player)
    {
        // Kunci dulu agar tidak trigger berulang
        isLocked = true;
        Debug.Log($"[Door] Player masuk pintu menuju {destinationRoomType}");
        DungeonManager.Instance?.OnPlayerEnterDoor(destinationRoomType, player);
    }
}