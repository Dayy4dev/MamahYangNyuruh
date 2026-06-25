using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public const int SLOT_UNARMED = 0;
    public const int SLOT_WEAPON_1 = 1;
    public const int SLOT_WEAPON_2 = 2;
    public const int TOTAL_SLOTS = 3;

    [Header("References")]
    [SerializeField] private PlayerAttack playerAttack;
    [Tooltip("Posisi di mana senjata yang dibuang di-spawn (kosongkan = pakai posisi player)")]
    [SerializeField] private Transform dropPoint;
    [Tooltip("Jarak deteksi WeaponPickup terdekat untuk tombol F")]
    [SerializeField] private float pickupRadius = 2f;
    [Tooltip("Layer yang dianggap sebagai WeaponPickup")]
    [SerializeField] private LayerMask pickupLayer;

    private WeaponData[] slots = new WeaponData[TOTAL_SLOTS];
    private int currentSlot = SLOT_UNARMED;
    private WeaponPickup nearestPickup;

    // Cache untuk optimasi performa visual senjata
    private Transform[] allChildrenCache;

    public UnityEvent<int, WeaponData> onSlotChanged;
    public UnityEvent<int> onActiveSlotChanged;
    public UnityEvent<WeaponPickup> onNearestPickupChanged;

    public int CurrentSlot => currentSlot;
    public WeaponData CurrentWeapon => slots[currentSlot];
    public WeaponData[] Slots => slots;

    void Awake()
    {
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();
    }

    void Start()
    {
        // FIX PERFORMA: Ambil referensi semua children SEKALI SAJA saat game dimulai
        // Hal ini menghindari stuttering/lag saat ganti senjata di komputer/HP spesifikasi rendah
        allChildrenCache = GetComponentsInChildren<Transform>(true);

        EquipSlot(SLOT_UNARMED);
    }

    void Update()
    {
        HandlePickupDetection();
        HandlePickupInput();
        HandleDropInput();
        HandleSlotSwitch();
    }

    public void TryPickup(WeaponPickup pickup)
    {
        WeaponData data = pickup.Data;
        int emptySlot = FindEmptyWeaponSlot();

        if (emptySlot != -1)
        {
            SetSlot(emptySlot, data);

            // --- FIX UI Gantung ---
            if (nearestPickup == pickup)
            {
                nearestPickup = null;
                onNearestPickupChanged?.Invoke(null);
            }

            pickup.OnPickedUp();
            SwitchToSlot(emptySlot);
        }
        else
        {
            if (currentSlot == SLOT_UNARMED)
            {
                DropFromSlot(SLOT_WEAPON_1);
                SetSlot(SLOT_WEAPON_1, data);

                // --- FIX UI Gantung ---
                if (nearestPickup == pickup)
                {
                    nearestPickup = null;
                    onNearestPickupChanged?.Invoke(null);
                }

                pickup.OnPickedUp();
                SwitchToSlot(SLOT_WEAPON_1);
            }
            else
            {
                DropFromSlot(currentSlot);
                SetSlot(currentSlot, data);

                // --- FIX UI Gantung ---
                if (nearestPickup == pickup)
                {
                    nearestPickup = null;
                    onNearestPickupChanged?.Invoke(null);
                }

                pickup.OnPickedUp();
                EquipSlot(currentSlot);
            }
        }
    }

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

        if (data.pickupPrefab != null)
        {
            // FIX LOGIC BUG: Amankan penentuan posisi spawn agar tidak NullReferenceException
            Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + transform.forward * 0.8f;

            if (dropPoint != null)
            {
                spawnPos.y = dropPoint.position.y;
            }

            GameObject dropped = Instantiate(data.pickupPrefab, spawnPos, Quaternion.identity);

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

    private void SwitchToSlot(int index)
    {
        if (index < 0 || index >= TOTAL_SLOTS) return;
        currentSlot = index;
        EquipSlot(currentSlot);
        onActiveSlotChanged?.Invoke(currentSlot);
    }

   private void EquipSlot(int index)
{
    WeaponData activeData = slots[index];

    if (playerAttack != null)
    {
        playerAttack.EquipWeaponData(activeData);
    }

    string activeWeaponName = activeData != null ? activeData.weaponName.Replace("_", "").Replace(" ", "").ToLower() : string.Empty;

    // Simpan referensi hitbox yang ditemukan di frame ini
    WeaponHitbox foundHitbox = null;

    foreach (Transform child in allChildrenCache)
    {
        if (child == null || child == transform) continue;

        string childName = child.gameObject.name.Replace("_", "").Replace(" ", "").ToLower();

        // LOGIKA UNTUK VISUAL DI TANGAN
        if (childName.EndsWith("hand"))
        {
            string weaponName = childName.Substring(0, childName.Length - 4);
            bool isWeaponActive = !string.IsNullOrEmpty(activeWeaponName) && activeWeaponName == weaponName;

            child.gameObject.SetActive(isWeaponActive);

            if (isWeaponActive)
            {
                // PERBAIKAN: Gunakan 'true' agar tetap mencari komponen meskipun objek/parent-nya sempat nonaktif
                foundHitbox = child.GetComponentInChildren<WeaponHitbox>(true);
            }
            continue;
        }

        // LOGIKA UNTUK VISUAL DI PUNGGUNG
        if (childName.EndsWith("back"))
        {
            string weaponName = childName.Substring(0, childName.Length - 4);

            bool isOwnedInSlot1 = slots[SLOT_WEAPON_1] != null && slots[SLOT_WEAPON_1].weaponName.Replace("_", "").Replace(" ", "").ToLower() == weaponName;
            bool isOwnedInSlot2 = slots[SLOT_WEAPON_2] != null && slots[SLOT_WEAPON_2].weaponName.Replace("_", "").Replace(" ", "").ToLower() == weaponName;
            bool isWeaponOwned = isOwnedInSlot1 || isOwnedInSlot2;

            bool isWeaponActive = !string.IsNullOrEmpty(activeWeaponName) && activeWeaponName == weaponName;
            bool shouldShowOnBack = isWeaponOwned && !isWeaponActive;

            child.gameObject.SetActive(shouldShowOnBack);
            continue;
        }
    }

    // PERBAIKAN UTAMA: Amankan pengiriman hitbox ke PlayerAttack setelah siklus foreach visual selesai
    if (playerAttack != null)
    {
        playerAttack.SetWeaponHitbox(foundHitbox);
        
        if (foundHitbox != null)
            Debug.Log($"[PlayerInventory] Berhasil mengirim Hitbox Melee: {foundHitbox.gameObject.name}");
        else if (activeData != null && activeData.weaponName != "Hand_Cannon" && activeData.weaponName != "HandCannon")
            Debug.LogWarning($"[PlayerInventory] Gagal menemukan WeaponHitbox di objek anak berakhiran 'hand' untuk senjata: {activeData.weaponName}");
    }
}
    private void HandleSlotSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToSlot(2);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchToSlot((currentSlot + 1) % TOTAL_SLOTS);
        }
        else if (scroll < 0f)
        {
            SwitchToSlot((currentSlot - 1 + TOTAL_SLOTS) % TOTAL_SLOTS);
        }
    }

    private void HandleDropInput()
    {
        if (Input.GetKeyDown(KeyCode.G))
            TryDrop();
    }

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}