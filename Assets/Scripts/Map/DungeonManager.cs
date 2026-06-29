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

    [Header("Room UI Bar Setup")]
    public RoomUIBar roomUIBar; // Tarik script UI Bar ke sini di Inspector
    private int maxEnemiesInRoom = 0;
    private int currentEnemiesInRoom = 0;

    [Header("Partner Wildcard Setup")]
    public GameObject partnerPrefab; // Seret Prefab Partner ke sini di Inspector
    [Range(0, 100)] public float partnerSpawnChance = 40f; // Peluang muncul (40%)

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

        float diceRoll = UnityEngine.Random.Range(0f, 100f);
        bool shouldPartnerSpawn = diceRoll <= partnerSpawnChance;

        if (shouldPartnerSpawn && partnerPrefab != null)
        {
            Debug.Log("[Wildcard Event] Partner muncul! Pintu ditahan sampai permen dipilih.");

            Vector3 spawnPos = Vector3.zero;
            Transform targetSpawnpointTransform = null; 
            bool spawnPointFound = false;

            Transform[] childTransforms = room.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child.CompareTag("PartnerSpawnpoint"))
                {
                    spawnPos = child.position;
                    targetSpawnpointTransform = child; 
                    spawnPointFound = true;
                    break;
                }
            }

            if (!spawnPointFound)
            {
                spawnPos = room.transform.position;
            }

            GameObject partnerObj = Instantiate(partnerPrefab, spawnPos, Quaternion.identity);

            if (spawnPointFound && targetSpawnpointTransform != null)
            {
                partnerObj.transform.parent = targetSpawnpointTransform;
                Debug.Log($"[DungeonManager] Partner berhasil dimasukkan ke dalam child {targetSpawnpointTransform.name}");
            }
            else
            {
                partnerObj.transform.parent = room.transform;
            }

            PartnerSystem partnerSys = partnerObj.GetComponent<PartnerSystem>();

            if (partnerSys != null)
            {
                partnerSys.OnCandySelected += () => { ExecuteRoomClearLogic(room); };
            }
        }
        else
        {
            ExecuteRoomClearLogic(room);
        }
    }

    // ───────────────────────────────────────────────────────────────────
    // LOGIKA UTAMA PERBAIKAN PORTAL BOSS ROOM
    // ───────────────────────────────────────────────────────────────────
    private void ExecuteRoomClearLogic(RoomController room)
    {
        LootboxRoomSpawner roomSpawner = room.GetComponentInChildren<LootboxRoomSpawner>();
        if (roomSpawner != null)
        {
            roomSpawner.SpawnRandomChest();
        }

        // PERBAIKAN: Jika ini adalah Boss Floor, langsung aktifkan portal yang ada di ruangan ini!
        if (FloorManager.Instance != null && FloorManager.Instance.IsBossFloor)
        {
            Debug.Log("[DungeonManager] Boss Floor terdeteksi selesai. Mengaktifkan Portal di Boss Room.");
            ActivatePortalInRoom(room);
            return; // Keluar agar tidak mengeksekusi switch-case RoomType.Bottom di bawah
        }

        switch (room.roomType)
        {
            case RoomType.Bottom:
                OpenExitDoorsOfRoom(RoomType.Bottom);
                break;

            case RoomType.Center:
                centerCleared = true;
                OpenExitDoorsOfRoom(RoomType.Center);
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

        switch (destination)
        {
            case RoomType.Center:
                if (roomMap.TryGetValue(RoomType.Bottom, out RoomController bottomRoom))
                {
                    foreach (var door in bottomRoom.exitDoors)
                    {
                        if (door != null) door.LockDoor();
                    }
                    Debug.Log("[DungeonManager] Bottom room doors locked (player masuk Center)");
                }
                break;

            case RoomType.Left:
            case RoomType.Right:
            case RoomType.Top:
                if (roomMap.TryGetValue(RoomType.Center, out RoomController centerRoom))
                {
                    foreach (var door in centerRoom.exitDoors)
                    {
                        if (door != null) door.LockDoor();
                    }
                    Debug.Log($"[DungeonManager] Center room doors locked (player masuk {destination})");
                }
                break;
        }

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
        {
            portal.Activate();
        }
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
        Vector3 targetPosition = spawnPoint != null
            ? spawnPoint.position
            : room.transform.position + fallbackOffset;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerTransform.position = targetPosition;
        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;

        currentRoom = room;
        currentRoomType = type;
        Debug.Log($"[DungeonManager] Player diteleport ke {type}.");

        EnemySpawner spawner = room.GetComponentInChildren<EnemySpawner>();
        if (spawner != null)
        {
            spawner.SpawnEnemies();
        }

        RefreshRoomEnemyCounter(type, room.gameObject);

        bool harusMengunci = (maxEnemiesInRoom > 0);

        if (room.roomState == RoomState.Active && harusMengunci)
        {
            if (room.exitDoors != null)
            {
                foreach (var door in room.exitDoors)
                {
                    if (door != null && !door.isLocked)
                    {
                        door.LockDoor();
                    }
                }
            }
        }

        foreach (Transform child in room.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("Block") || child.name.Contains("Blocker"))
            {
                if (child.gameObject.activeSelf)
                {
                    Vector3 currentPos = child.transform.localPosition;
                    child.transform.localPosition = new Vector3(currentPos.x, 0f, currentPos.z);

                    Vector3 currentScale = child.transform.localScale;
                    child.transform.localScale = new Vector3(currentScale.x, 40f, currentScale.z);

                    BoxCollider boxCol = child.GetComponent<BoxCollider>();
                    if (boxCol != null)
                    {
                        boxCol.isTrigger = false;
                    }
                }
            }
        }
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

        if (FloorManager.Instance != null)
            FloorManager.Instance.AdvanceFloor();

        foreach (var room in FindObjectsOfType<RoomController>())
            Destroy(room.gameObject);

        roomMap.Clear();
        centerCleared = false;
        currentRoomType = RoomType.Bottom;

        yield return null;

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
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        yield return wait; 
        TeleportPlayerToRoom(RoomType.Bottom);

        Debug.Log("[DungeonManager] Map baru loaded!");
    }

    void SpawnBossRoom()
    {
        GameObject bossRoomObj = Instantiate(bossRoomPrefab, Vector3.zero, Quaternion.identity);
        bossRoomObj.name = "BossRoom";

        RoomController roomCtrl = bossRoomObj.GetComponent<RoomController>();
        if (roomCtrl != null)
        {
            roomCtrl.roomType = RoomType.Bottom;
            roomCtrl.GridPosition = Vector2Int.zero;
        }
    }

    public void RefreshRoomEnemyCounter(RoomType type, GameObject roomObject)
    {
        EnemySpawner spawner = roomObject.GetComponentInChildren<EnemySpawner>();

        if (spawner != null)
        {
            maxEnemiesInRoom = spawner.spawnCount;
        }
        else
        {
            maxEnemiesInRoom = 0;
        }

        currentEnemiesInRoom = maxEnemiesInRoom;

        Debug.Log($"[DungeonManager] Room {type} sukses disinkronkan. Total musuh: {maxEnemiesInRoom}");

        if (maxEnemiesInRoom > 0)
        {
            if (roomUIBar != null)
            {
                roomUIBar.ShowBar(true);
                roomUIBar.UpdateRoomText(type.ToString());
                roomUIBar.UpdateBarValue(currentEnemiesInRoom, maxEnemiesInRoom);
            }

            if (currentRoom != null && currentRoom.exitDoors != null)
            {
                foreach (var door in currentRoom.exitDoors)
                {
                    if (door != null)
                    {
                        door.LockDoor();
                        Debug.Log($"[DungeonManager] Kunci pengaman dipaksa AKTIF pada: {door.name}");
                    }
                }
            }
        }
        else
        {
            if (roomUIBar != null) roomUIBar.ShowBar(false);
            OpenExitDoorsOfRoom(type);
        }
    }

    public void OnEnemyKilled()
    {
        currentEnemiesInRoom--;
        if (currentEnemiesInRoom < 0) currentEnemiesInRoom = 0;

        if (roomUIBar != null)
        {
            roomUIBar.UpdateBarValue(currentEnemiesInRoom, maxEnemiesInRoom);
        }
    }
}