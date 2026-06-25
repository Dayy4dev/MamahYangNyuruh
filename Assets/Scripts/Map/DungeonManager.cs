using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("References")]
    public DungeonGenerator dungeonGenerator;
    public Transform playerTransform;

    [Header("Boss Floor")]
    public GameObject bossRoomPrefab;   // Prefab khusus boss room, di-spawn di floor 4

    [Header("Spawn Settings")]
    public string spawnPointName = "SpawnPoint";
    public Vector3 fallbackOffset = new Vector3(0, 1, 0);

    [Header("Transition")]
    public float transitionDelay = 0.5f;

    // Internal state
    private RoomController currentRoom;
    private Dictionary<RoomType, RoomController> roomMap = new Dictionary<RoomType, RoomController>();
    private RoomType currentRoomType = RoomType.Bottom;
    private bool centerCleared = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeDungeon());
    }

    // ─────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────

    IEnumerator InitializeDungeon()
    {
        dungeonGenerator.GenerateDungeon();
        yield return null;
        RegisterAllRooms();
        TeleportPlayerToRoom(RoomType.Bottom);

        int floor = FloorManager.Instance != null ? FloorManager.Instance.CurrentFloor : 1;
        Debug.Log($"[DungeonManager] Floor {floor} dimulai.");
    }

    void RegisterAllRooms()
    {
        roomMap.Clear();
        foreach (var room in FindObjectsOfType<RoomController>())
        {
            if (!roomMap.ContainsKey(room.roomType))
                roomMap[room.roomType] = room;
            else
                Debug.LogWarning($"[DungeonManager] Duplikat room type: {room.roomType}");
        }
        Debug.Log($"[DungeonManager] {roomMap.Count} room terdaftar.");
    }

    // ─────────────────────────────────────────
    // CALLBACK DARI RoomController
    // ─────────────────────────────────────────

    public void OnRoomCleared(RoomController room)
    {
        Debug.Log($"[DungeonManager] {room.roomType} cleared!");

        switch (room.roomType)
        {
            case RoomType.Bottom:
                OpenExitDoorsOfRoom(RoomType.Bottom);
                break;

            case RoomType.Center:
                centerCleared = true;
                OpenExitDoorsOfRoom(RoomType.Center);
                Debug.Log("[DungeonManager] 3 pilihan room terbuka!");
                break;

            case RoomType.Left:
            case RoomType.Right:
            case RoomType.Top:
                ActivatePortalInRoom(room);
                break;
        }
    }

    // ─────────────────────────────────────────
    // CALLBACK DARI DoorController
    // ─────────────────────────────────────────

    public void OnPlayerEnterDoor(RoomType destination, GameObject player)
    {
        if (!roomMap.ContainsKey(destination))
        {
            Debug.LogWarning($"[DungeonManager] Room {destination} tidak ditemukan!");
            return;
        }

        currentRoomType = destination;
        currentRoom = roomMap[destination];
        currentRoom.ActivateRoom();
        TeleportPlayerToRoom(destination);
    }

    // ─────────────────────────────────────────
    // CALLBACK DARI NextMapPortal
    // ─────────────────────────────────────────

    public void GoToNextMap()
    {
        StartCoroutine(LoadNextMap());
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────

    void OpenExitDoorsOfRoom(RoomType type)
    {
        if (roomMap.TryGetValue(type, out RoomController room))
            foreach (var door in room.exitDoors)
                if (door != null) door.UnlockDoor();
    }

    void ActivatePortalInRoom(RoomController room)
    {
        NextMapPortal portal = room.GetComponentInChildren<NextMapPortal>(true);
        if (portal != null)
            portal.Activate();
        else
        {
            Debug.LogWarning($"[DungeonManager] Tidak ada NextMapPortal di {room.roomType}! Langsung load map.");
            StartCoroutine(LoadNextMap());
        }
    }

    void TeleportPlayerToRoom(RoomType type)
    {
        if (playerTransform == null) { Debug.LogWarning("[DungeonManager] playerTransform belum di-assign!"); return; }
        if (!roomMap.TryGetValue(type, out RoomController room)) { Debug.LogWarning($"[DungeonManager] Room {type} tidak ditemukan!"); return; }

        Transform spawnPoint = FindSpawnPoint(room.transform);
        playerTransform.position = spawnPoint != null
            ? spawnPoint.position
            : room.transform.position + fallbackOffset;

        currentRoom = room;
        currentRoomType = type;
        Debug.Log($"[DungeonManager] Player diteleport ke {type}.");
    }

    Transform FindSpawnPoint(Transform parent)
    {
        Transform direct = parent.Find(spawnPointName);
        if (direct != null) return direct;
        foreach (Transform child in parent)
        {
            Transform found = FindSpawnPoint(child);
            if (found != null) return found;
        }
        return null;
    }

    IEnumerator LoadNextMap()
    {
        Debug.Log("[DungeonManager] Memuat map berikutnya...");
        yield return new WaitForSeconds(transitionDelay);

        // Advance floor
        if (FloorManager.Instance != null)
            FloorManager.Instance.AdvanceFloor();

        // Hapus semua room lama
        foreach (var room in FindObjectsOfType<RoomController>())
            Destroy(room.gameObject);

        roomMap.Clear();
        centerCleared = false;
        currentRoomType = RoomType.Bottom;

        yield return null;

        // Cek apakah ini boss floor
        bool isBossFloor = FloorManager.Instance != null && FloorManager.Instance.IsBossFloor;

        if (isBossFloor && bossRoomPrefab != null)
        {
            Debug.Log("[DungeonManager] BOSS FLOOR! Spawning boss room...");
            SpawnBossRoom();
        }
        else
        {
            dungeonGenerator.GenerateDungeon();
        }

        yield return null;

        RegisterAllRooms();
        TeleportPlayerToRoom(RoomType.Bottom);

        Debug.Log("[DungeonManager] Map baru loaded!");
    }

    void SpawnBossRoom()
    {
        // Spawn boss room di posisi (0,0,0) sebagai Bottom room
        GameObject bossRoomObj = Instantiate(bossRoomPrefab, Vector3.zero, Quaternion.identity);
        bossRoomObj.name = "BossRoom";

        RoomController roomCtrl = bossRoomObj.GetComponent<RoomController>();
        if (roomCtrl != null)
        {
            roomCtrl.roomType = RoomType.Bottom;
            roomCtrl.GridPosition = Vector2Int.zero;
        }
    }
}