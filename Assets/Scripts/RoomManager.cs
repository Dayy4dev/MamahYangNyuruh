using System.Collections;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public Transform player;

    private void Awake()
    {
        Instance = this;
    }

    public void EnterRoom(RoomController room)
    {
        StartCoroutine(EnterRoomRoutine(room));
    }

    IEnumerator EnterRoomRoutine(RoomController room)
    {
        yield return FadeOut();

        player.position = room.playerSpawn.position;

        yield return FadeIn();

        if (room.isCombatRoom && !room.visited)
        {
            room.visited = true;

            CombatRoom combat =
                room.GetComponent<CombatRoom>();

            if(combat != null)
                combat.StartCombat();
        }
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.25f);
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(0.25f);
    }
}