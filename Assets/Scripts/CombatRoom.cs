using UnityEngine;

// Attach ke prefab room yang punya enemy & door blocker.
// StartCombat() dan EndCombat() dipanggil otomatis oleh RoomController.

public class CombatRoom : MonoBehaviour
{
    [Header("Combat Setup")]
    // Kosongkan jika pakai auto-detect dari RoomController (tag-based)
    public GameObject[] enemies;

    [Header("Door Blocker")]
    public GameObject doorBlocker;

    public void StartCombat()
    {
        if (doorBlocker != null)
            doorBlocker.SetActive(true);

        foreach (GameObject enemy in enemies)
            if (enemy != null) enemy.SetActive(true);

        Debug.Log($"[CombatRoom] Combat dimulai di {gameObject.name}");
    }

    public void EndCombat()
    {
        if (doorBlocker != null)
            doorBlocker.SetActive(false);

        Debug.Log($"[CombatRoom] Combat selesai di {gameObject.name}");
    }
}