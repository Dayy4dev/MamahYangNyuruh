using UnityEngine;

public class RoomController : MonoBehaviour
{
    public Transform playerSpawn;

    [Header("Combat")]
    public bool isCombatRoom;

    [HideInInspector]
    public bool visited;

    public Vector2Int GridPosition { get; internal set; }
}