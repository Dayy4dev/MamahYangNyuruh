using UnityEngine;

// Taruh di GameManager (satu GameObject dengan DungeonManager & DungeonGenerator)
// FloorManager tracking floor saat ini dan kasih info ke sistem lain

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    [Header("Settings")]
    public int bossFloor = 4;

    // Floor saat ini, mulai dari 1
    public int CurrentFloor { get; private set; } = 1;
    public bool IsBossFloor => CurrentFloor >= bossFloor;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AdvanceFloor()
    {
        CurrentFloor++;
        Debug.Log($"[FloorManager] Floor {CurrentFloor}" + (IsBossFloor ? " — BOSS FLOOR!" : ""));
    }

    public void ResetFloor()
    {
        CurrentFloor = 1;
        Debug.Log("[FloorManager] Floor direset ke 1.");
    }

    // ── Scaling helpers ──────────────────────────────────────────────────────

    // Multiplier HP berdasarkan floor: floor 1 = 1x, floor 2 = 1.3x, dst
    public float GetHPMultiplier()
    {
        return 1f + (CurrentFloor - 1) * 0.3f;
    }

    // Multiplier damage berdasarkan floor
    public float GetDamageMultiplier()
    {
        return 1f + (CurrentFloor - 1) * 0.2f;
    }

    // Multiplier speed berdasarkan floor (cap di 1.5x agar tidak terlalu cepat)
    public float GetSpeedMultiplier()
    {
        return Mathf.Min(1f + (CurrentFloor - 1) * 0.1f, 1.5f);
    }
}