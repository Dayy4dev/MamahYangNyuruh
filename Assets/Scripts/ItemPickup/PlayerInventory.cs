using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Mengelola hotbar player: slot 0 = Unarmed (permanent), slot 1-2 = senjata.
/// Pickup  : tekan F saat berada di dekat WeaponPickup
/// Drop    : tekan G untuk menjatuhkan senjata yang sedang dipegang
/// Switch  : scroll mouse / tombol 1-3
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    public const int SLOT_UNARMED  = 0;
    public const int SLOT_WEAPON_1 = 1;
    public const int SLOT_WEAPON_2 = 2;
    public const int TOTAL_SLOTS   = 3;

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;

    [Tooltip("Posisi di mana senjata yang dibuang di-spawn (kosongkan = pakai posisi player)")]
    [SerializeField] private Transform dropPoint;

    [Tooltip("Jarak deteksi WeaponPickup terdekat untuk tombol F")]
    [SerializeField] private float pickupRadius = 2f;

    [Tooltip("Layer yang dianggap sebagai WeaponPickup")]
    [SerializeField] private LayerMask pickupLayer;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    // slots[0] = Unarmed (WeaponData == null, tidak bisa di-drop)
    // slots[1-2] = senjata biasa (null = kosong)
    private WeaponData[] slots = new WeaponData[TOTAL_SLOTS];

    private int currentSlot = SLOT_UNARMED;

    // Pickup yang paling dekat dengan player (diupdate tiap frame)
    private WeaponPickup nearestPickup;

    // -------------------------------------------------------------------------
    // Events  (UI dapat subscribe ke sini)
    // -------------------------------------------------------------------------

    public UnityEvent<int, WeaponData> onSlotChanged;   // (slotIndex, data)  dipanggil saat isi slot berubah
    public UnityEvent<int> onActiveSlotChanged;          // (slotIndex)        dipanggil saat slot aktif berubah
    public UnityEvent<WeaponPickup> onNearestPickupChanged; // untuk UI prompt

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public int         CurrentSlot   => currentSlot;
    public WeaponData  CurrentWeapon => slots[currentSlot];
    public WeaponData[] Slots        => slots;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();
    }

    void Start()
    {
        // Slot 0 selalu Unarmed — tidak perlu WeaponData, cukup null sebagai penanda
        EquipSlot(SLOT_UNARMED);
    }

    void Update()
    {
        HandlePickupDetection();
        HandlePickupInput();
        HandleDropInput();
        HandleSlotSwitch();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Dipanggil WeaponPickup saat player menekan F.</summary>
    public void TryPickup(WeaponPickup pickup)
    {
        WeaponData data = pickup.Data;

        // Cari slot kosong dulu (slot 1 atau 2)
        int emptySlot = FindEmptyWeaponSlot();

        if (emptySlot != -1)
        {
            // Ada slot kosong → langsung ambil
            SetSlot(emptySlot, data);
            pickup.OnPickedUp();
            SwitchToSlot(emptySlot);
        }
        else
        {
            // Slot penuh → tukar dengan senjata yang sedang dipegang
            if (currentSlot == SLOT_UNARMED)
            {
                // Sedang unarmed, tukar slot 1
                DropFromSlot(SLOT_WEAPON_1);
                SetSlot(SLOT_WEAPON_1, data);
                pickup.OnPickedUp();
                SwitchToSlot(SLOT_WEAPON_1);
            }
            else
            {
                // Tukar senjata di slot aktif
                DropFromSlot(currentSlot);
                SetSlot(currentSlot, data);
                pickup.OnPickedUp();
                // Tetap di slot yang sama
                EquipSlot(currentSlot);
            }
        }
    }

    /// <summary>Drop senjata di slot aktif (dipanggil tombol G).</summary>
    public void TryDrop()
    {
        if (currentSlot == SLOT_UNARMED)
        {
            Debug.Log("[PlayerInventory] Tidak bisa membuang Unarmed.");
            return;
        }

        if (slots[currentSlot] == null)
        {
            Debug.Log("[PlayerInventory] Slot kosong, tidak ada yang bisa dibuang.");
            return;
        }

        DropFromSlot(currentSlot);
        SwitchToSlot(SLOT_UNARMED);
    }

    // -------------------------------------------------------------------------
    // Private — Slot Management
    // -------------------------------------------------------------------------

    private void SetSlot(int index, WeaponData data)
    {
        slots[index] = data;
        onSlotChanged?.Invoke(index, data);
        Debug.Log($"[PlayerInventory] Slot {index} = {(data != null ? data.weaponName : "Empty")}");
    }

    private void DropFromSlot(int index)
    {
        WeaponData data = slots[index];
        if (data == null) return;

        // Spawn pickup prefab di dunia
        if (data.pickupPrefab != null)
        {
            Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + transform.forward * 0.8f;
            spawnPos.y = dropPoint.position.y;

            GameObject dropped = Instantiate(data.pickupPrefab, spawnPos, Quaternion.identity);

            // Tandai sebagai "dropped" agar dihapus saat pindah scene
            if (dropped.TryGetComponent<WeaponPickup>(out WeaponPickup wp))
                wp.MarkAsDropped();
        }
        else
        {
            Debug.LogWarning($"[PlayerInventory] {data.weaponName} tidak punya pickupPrefab!");
        }

        SetSlot(index, null);
    }

    private int FindEmptyWeaponSlot()
    {
        for (int i = SLOT_WEAPON_1; i <= SLOT_WEAPON_2; i++)
        {
            if (slots[i] == null) return i;
        }
        return -1;
    }

    // -------------------------------------------------------------------------
    // Private — Equip
    // -------------------------------------------------------------------------

    private void SwitchToSlot(int index)
    {
        if (index < 0 || index >= TOTAL_SLOTS) return;
        currentSlot = index;
        EquipSlot(currentSlot);
        onActiveSlotChanged?.Invoke(currentSlot);
    }

private void EquipSlot(int index)
{
    WeaponData data = slots[index];
    
    // 1. Kirim data stats ke PlayerAttack bawaan aslimu
    if (playerAttack != null)
    {
        playerAttack.EquipWeaponData(data);
    }

    // Ambil semua objek di bawah Player (termasuk yang non-aktif)
    Transform[] allChildren = GetComponentsInChildren<Transform>(true);

    // Format nama data senjata saat ini agar bersih
    string cleanDataName = data != null ? data.weaponName.Replace("_", "").Replace(" ", "").ToLower() : "";

    foreach (Transform child in allChildren)
    {
        // PENTING: Jangan pernah utak-atik atau mematikan objek Player utama itu sendiri!
        if (child == transform) continue;

        // Daftar objek yang BENAR-BENAR merupakan root/induk dari senjata kamu di tangan
        // Tambahkan nama GameObject senjata barumu di sini nanti (misal: "BalloonSword")
        bool isActualWeaponRoot = child.gameObject.name == "ToyHammer" || 
                                  child.gameObject.name == "HandCannon" || 
                                  child.gameObject.name == "BalloonSword" ||
                                  child.gameObject.name == "Unarmed";

        // Jika objek yang sedang dicek BUKAN salah satu dari daftar senjata di atas,
        // biarkan saja tetap menyala (jangan di-SetActive false) agar character tidak hilang!
        if (!isActualWeaponRoot) continue;

        // Samakan format nama objek senjata untuk dicocokkan dengan WeaponData
        string cleanChildName = child.gameObject.name.Replace("_", "").Replace(" ", "").ToLower();

        if (data != null && cleanChildName == cleanDataName)
        {
            // NYALAKAN senjata yang sedang dipilih/di-pickup
            child.gameObject.SetActive(true);

            // Jika senjata Melee ini punya Hitbox, hubungkan ke PlayerAttack agar bisa memberi damage
            WeaponHitbox hitbox = child.GetComponentInChildren<WeaponHitbox>(true);
            if (playerAttack != null && hitbox != null)
            {
                playerAttack.SetWeaponHitbox(hitbox);
            }
            
            continue;
        }

        // MATIKAN hanya objek root senjata yang tidak sedang dipilih
        child.gameObject.SetActive(false);
    }

    // Jika beralih ke Unarmed (tangan kosong), bersihkan referensi hitbox di PlayerAttack
    if (data == null && playerAttack != null)
    {
        playerAttack.SetWeaponHitbox(null);
    }
}
    // -------------------------------------------------------------------------
    // Private — Input
    // -------------------------------------------------------------------------

   private void HandleSlotSwitch()
{
    if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSlot(0);
    if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSlot(1);
    if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToSlot(2);

    // FIX: Menggunakan GetAxisRaw dan ambang batas (threshold) agar tidak infinite loop
    float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
    if (scroll > 0.1f)
    {
        SwitchToSlot((currentSlot + 1) % TOTAL_SLOTS);
    }
    else if (scroll < -0.1f)
    {
        SwitchToSlot((currentSlot - 1 + TOTAL_SLOTS) % TOTAL_SLOTS);
    }
}

    private void HandleDropInput()
    {
        if (Input.GetKeyDown(KeyCode.G))
            TryDrop();
    }

    // -------------------------------------------------------------------------
    // Private — Pickup Detection
    // -------------------------------------------------------------------------

    private void HandlePickupDetection()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRadius, pickupLayer);

        WeaponPickup closest = null;
        float closestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent<WeaponPickup>(out WeaponPickup wp)) continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = wp;
            }
        }

        if (closest != nearestPickup)
        {
            nearestPickup = closest;
            onNearestPickupChanged?.Invoke(nearestPickup);
        }
    }

    private void HandlePickupInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && nearestPickup != null)
            TryPickup(nearestPickup);
    }

    // -------------------------------------------------------------------------
    // Gizmos
    // -------------------------------------------------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}