using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Unique Rooms")]
    public GameObject spawnRoom;
    public GameObject shopRoom;
    public GameObject eliteRoom;
    public GameObject rewardRoom;
    public GameObject bossRoom;
    public GameObject eventRoom;

    [Header("Combat Rooms")]
    public GameObject[] easyCombatRooms;
    public GameObject[] mediumCombatRooms;
    public GameObject[] hardCombatRooms;

    [Header("Settings")]
    public float roomSize = 30f;

    private Dictionary<Vector2Int, GameObject> rooms = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        // Selalu bersihkan dictionary sebelum generate ulang (jika ada fitur reset)
        rooms.Clear();

        // 1. Spawn Room (Pusat)
        Vector2Int spawnPos = Vector2Int.zero;
        CreateRoom(spawnRoom, spawnPos);

        // 2. Jalur Kiri (Horizontal)
        CreateRoom(GetRandomRoom(easyCombatRooms), new Vector2Int(-1, 0));
        CreateRoom(shopRoom, new Vector2Int(-2, 0));

        // 3. Jalur Kanan (Horizontal)
        CreateRoom(GetRandomRoom(easyCombatRooms), new Vector2Int(1, 0));
        CreateRoom(eventRoom, new Vector2Int(2, 0));

        // 4. Jalur Bawah (Elite)
        CreateRoom(eliteRoom, new Vector2Int(0, -1));

        // 5. Jalur Atas & Sayap Tengah
        CreateRoom(rewardRoom, new Vector2Int(0, 1));
        CreateRoom(GetRandomRoom(mediumCombatRooms), new Vector2Int(-1, 1));
        CreateRoom(GetRandomRoom(mediumCombatRooms), new Vector2Int(1, 1));

        // 6. Jalur Menuju Boss
        CreateRoom(bossRoom, new Vector2Int(0, 2));
        CreateRoom(GetRandomRoom(hardCombatRooms), new Vector2Int(0, 3)); 
        // Catatan: Jika ingin Hard Combat sebelum Boss, tukar koordinat boss menjadi (0, 3) dan hardCombat menjadi (0, 2)
    }

    GameObject GetRandomRoom(GameObject[] roomPool)
    {
        if (roomPool == null || roomPool.Length == 0) return null;
        return roomPool[Random.Range(0, roomPool.Length)];
    }

    void CreateRoom(GameObject prefab, Vector2Int gridPos)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"Prefab kosong pada koordinat {gridPos}!");
            return;
        }

        // PENCEGAHAN TUMPUK: Cek apakah koordinat grid sudah terisi
        if (rooms.ContainsKey(gridPos))
        {
            Debug.LogError($"[Dungeon Error] Koordinat {gridPos} sudah diisi oleh {rooms[gridPos].name}. Gagal menimpa dengan {prefab.name}!");
            return; // Batalkan pembuatan ruangan agar tidak menumpuk
        }

        Vector3 worldPos = new Vector3(
            gridPos.x * roomSize,
            0,
            gridPos.y * roomSize
        );

        GameObject room = Instantiate(prefab, worldPos, Quaternion.identity);
        room.name = $"{prefab.name} [{gridPos.x}, {gridPos.y}]"; // Mengubah nama di hierarchy agar mudah di-debug

        RoomController roomScript = room.GetComponent<RoomController>();
        if (roomScript != null)
            roomScript.GridPosition = gridPos;

        rooms.Add(gridPos, room);
    }
}
