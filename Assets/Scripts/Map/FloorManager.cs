using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

    // Hapus variabel: public int bossFloor = 4;

    // Floor saat ini, mulai dari 1
    public int CurrentFloor { get; private set; } = 1;

    // UBAH DI SINI: Menggunakan Modulo (%) agar setiap kelipatan 4 jadi boss floor
    public bool IsBossFloor => CurrentFloor % 4 == 0;

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

    // Scaling stat musuh akan otomatis terus meningkat tanpa batas seiring naiknya floor
    public float GetHPMultiplier() { return 1f + (CurrentFloor - 1) * 0.3f; }
    public float GetDamageMultiplier() { return 1f + (CurrentFloor - 1) * 0.2f; }
    public float GetSpeedMultiplier() { return Mathf.Min(1f + (CurrentFloor - 1) * 0.1f, 1.5f);}
    
}