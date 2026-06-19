using UnityEngine;
using System.Collections.Generic;

public class FloorGenerator : MonoBehaviour
{
    public GameObject startRoom;
    public List<GameObject> combatRooms;
    public GameObject eliteRoom;
    public GameObject bossRoom;

    private Vector3 currentPos;

    void Start()
    {
        GenerateFloor();
    }

    void GenerateFloor()
    {
        currentPos = Vector3.zero;

        SpawnRoom(startRoom);

        for(int i = 0; i < 4; i++)
        {
            GameObject room =
                combatRooms[
                    Random.Range(0, combatRooms.Count)];

            currentPos += new Vector3(25,0,0);

            SpawnRoom(room);
        }

        currentPos += new Vector3(25,0,0);
        SpawnRoom(eliteRoom);

        currentPos += new Vector3(25,0,0);
        SpawnRoom(bossRoom);
    }

    void SpawnRoom(GameObject room)
    {
        Instantiate(room, currentPos, Quaternion.identity);
    }
}