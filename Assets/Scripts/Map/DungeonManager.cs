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
            Transform targetSpawnpointTransform = null; // Variabel baru untuk menyimpan referensi transform spawnpoint
            bool spawnPointFound = false;

            Transform[] childTransforms = room.GetComponentsInChildren<Transform>();
            foreach (Transform child in childTransforms)
            {
                if (child.CompareTag("PartnerSpawnpoint"))
                {
                    spawnPos = child.position;
                    targetSpawnpointTransform = child; // Ambil transform dari spawnpoint ini
                    spawnPointFound = true;
                    break;
                }
            }

            if (!spawnPointFound)
            {
                spawnPos = room.transform.position; 
            }

            // 1. Spawn partner seperti biasa
            GameObject partnerObj = Instantiate(partnerPrefab, spawnPos, Quaternion.identity);

            // 2. JADIKAN PARTNER SEBAGAI CHILD NYA SPAWNPOINT
            if (spawnPointFound && targetSpawnpointTransform != null)
            {
                partnerObj.transform.parent = targetSpawnpointTransform;
                Debug.Log($"[DungeonManager] Partner berhasil dimasukkan ke dalam child {targetSpawnpointTransform.name}");
            }
            else
            {
                // Fallback: jika spawnpoint tidak ketemu, masukkan langsung ke parent ruangan agar tetap ke-delete
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

    // 2. Buat fungsi bantuan baru ini untuk menampung logika switch-case asli milikmu
    private void ExecuteRoomClearLogic(RoomController room)
    {
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

        // ─────────────────────────────────────────
        // LOCK PARENT ROOM DOORS BASED ON DESTINATION
        // ─────────────────────────────────────────
        switch (destination)
        {
            case RoomType.Center:
                // Jika masuk Center, lock Bottom room doors
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
                // Jika masuk Left/Right/Top, lock Center room doors
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

    // 1. Jalankan Spawner musuh terlebih dahulu
    EnemySpawner spawner = room.GetComponentInChildren<EnemySpawner>();
    if (spawner != null)
    {
        spawner.SpawnEnemies(); 
    }

    // 2. Hitung jumlah musuh di ruangan ini
    RefreshRoomEnemyCounter(type, room.gameObject);

    // 3. TENTUKAN STATUS PINTU BERDASARKAN ADA/TIDAKNYA MUSUH
    // NOTE: Door lock state sudah di-set oleh ActivateRoom(). DoorBlocker di-manage oleh DoorController.RefreshVisual()
    // Jangan override di sini, cukup pastikan room state konsisten.
    bool harusMengunci = (maxEnemiesInRoom > 0);

    // Atur status kunci pada komponen pintu HANYA jika belum ter-set (cegah duplikat lock/unlock)
    if (room.roomState == RoomState.Active && harusMengunci)
    {
        // Ensure doors are locked dan DoorBlocker active jika ada musuh
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

    // 4. ATUR POSISI/SKALA BLOCKER JIKA DIPERLUKAN (tapi jangan disable berdasarkan harusMengunci)
    // Karena DoorBlocker state sudah di-manage oleh LockDoor()/UnlockDoor()
    foreach (Transform child in room.GetComponentsInChildren<Transform>(true))
    {
        if (child.name.Contains("Block") || child.name.Contains("Blocker"))
        {
            // Hanya atur posisi/skala jika blockernya sedang aktif (tidak disable/enable di sini)
            if (child.gameObject.activeSelf)
            {
                // Menyesuaikan posisi Y agar pas menapak di lantai dasar prefab
                Vector3 currentPos = child.transform.localPosition;
                child.transform.localPosition = new Vector3(currentPos.x, 0f, currentPos.z);

                // Mengatur tinggi tembok pelindung agar pas (tidak terlalu ekstrem raksasa)
                Vector3 currentScale = child.transform.localScale;
                child.transform.localScale = new Vector3(currentScale.x, 40f, currentScale.z);
                
                // Pastikan collider keras dan tidak tembus
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
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        yield return wait; // Tunggu frame berikutnya agar semua room ter-initialize
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

    // Fungsi yang dipanggil setiap kali player masuk room baru untuk menghitung musuh
public void RefreshRoomEnemyCounter(RoomType type, GameObject roomObject)
{
    // 1. Ambil komponen EnemySpawner langsung dari ruangan ini
    EnemySpawner spawner = roomObject.GetComponentInChildren<EnemySpawner>();

    // 2. Jika spawner ditemukan, gunakan nilai spawnCount aslinya agar akurat tanpa delay frame
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

    // 3. Jika ada musuh yang harus dilawan (> 0)
    if (maxEnemiesInRoom > 0)
    {
        if (roomUIBar != null)
        {
            roomUIBar.ShowBar(true);
            roomUIBar.UpdateRoomText(type.ToString());
            roomUIBar.UpdateBarValue(currentEnemiesInRoom, maxEnemiesInRoom);
        }

        // FORCE LOCK: Paksa semua pintu di ruangan aktif saat ini untuk memunculkan Block!
        if (currentRoom != null && currentRoom.exitDoors != null)
        {
            foreach (var door in currentRoom.exitDoors)
            {
                if (door != null)
                {
                    door.LockDoor(); // Ini akan memanggil SetActive(true) pada objek 'Block'
                    Debug.Log($"[DungeonManager] Kunci pengaman dipaksa AKTIF pada: {door.name}");
                }
            }
        }
    }
    else
    {
        // Jika memang ruangan kosong (seperti Room awal/Bottom), buka pintunya
        if (roomUIBar != null) roomUIBar.ShowBar(false);
        OpenExitDoorsOfRoom(type);
    }
}
    // Fungsi yang akan dipanggil oleh EnemySpawner saat musuh mati
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