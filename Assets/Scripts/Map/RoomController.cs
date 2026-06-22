using System.Collections.Generic;
using UnityEngine;

// RoomType dan RoomState ada di RoomEnums.cs

public class RoomController : MonoBehaviour
{
    [Header("Room Info")]
    public RoomType roomType;
    public RoomState roomState = RoomState.Locked;

    [Header("Doors")]
    public DoorController[] exitDoors;

    [Header("Enemy Detection")]
    public bool autoDetectEnemies = true;
    public string enemyTag = "Enemy";
    public List<GameObject> enemies = new List<GameObject>();

    // Properties
    public Vector2Int GridPosition { get; set; }
    public string ZoneName { get; set; }

    private bool hasCleared = false;
    private CombatRoom combatRoom;

    void Start()
    {
        combatRoom = GetComponent<CombatRoom>();

        if (autoDetectEnemies)
            DetectEnemies();

        if (roomType == RoomType.Bottom)
            ActivateRoom();
        else
            LockRoom();
    }

    void Update()
    {
        if (roomState == RoomState.Active && !hasCleared)
            CheckEnemyCleared();
    }

    void DetectEnemies()
    {
        enemies.Clear();
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            if (enemy.transform.IsChildOf(this.transform))
                enemies.Add(enemy);
        }
    }

    void CheckEnemyCleared()
    {
        enemies.RemoveAll(e => e == null);
        if (enemies.Count == 0)
            OnRoomCleared();
    }

    public void ActivateRoom()
    {
        roomState = RoomState.Active;

        foreach (var door in exitDoors)
            if (door != null) door.LockDoor();

        if (combatRoom != null)
            combatRoom.StartCombat();

        if (autoDetectEnemies)
        {
            DetectEnemies();
            if (enemies.Count == 0)
                OnRoomCleared();
        }

        Debug.Log($"[Room] {roomType} aktif.");
    }

    public void LockRoom()
    {
        roomState = RoomState.Locked;
        foreach (var door in exitDoors)
            if (door != null) door.LockDoor();
    }

    void OnRoomCleared()
    {
        if (hasCleared) return;
        hasCleared = true;
        roomState = RoomState.Cleared;

        if (combatRoom != null)
            combatRoom.EndCombat();

        foreach (var door in exitDoors)
            if (door != null) door.UnlockDoor();

        Debug.Log($"[Room] {roomType} cleared!");
        DungeonManager.Instance?.OnRoomCleared(this);
    }

    public void ForceRoomClear()
    {
        enemies.Clear();
        OnRoomCleared();
    }
}