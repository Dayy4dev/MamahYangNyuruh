using UnityEngine;

// Attach script ini ke prefab room yang punya enemy & door blocker.
// RoomController akan otomatis memanggil StartCombat() dan EndCombat().
public class CombatRoom : MonoBehaviour
{
    [Header("Combat Setup")]
    // Enemy yang di-spawn/aktifkan saat combat mulai
    // Kosongkan jika pakai auto-detect dari RoomController (tag-based)
    public GameObject[] enemies;

    [Header("Door Blocker")]
    // GameObject yang menghalangi pintu selama combat (misal: barrier/portal tertutup)
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