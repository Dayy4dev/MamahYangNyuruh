using UnityEngine;

public class TriggerTeleport : MonoBehaviour
{
    public RoomController targetRoom;

    private bool entered;

    private void OnTriggerEnter(Collider other)
    {
        if (entered) return;

        if (!other.CompareTag("Player"))
            return;

        entered = true;

        RoomManager.Instance.EnterRoom(targetRoom);
    }
}