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

    // --- FITUR KECEPATAN ---
    [Header("Movement Speed Setup")]
    [Tooltip("Tarik komponen PlayerMovement dari GameObject Player ke sini")]
    [SerializeField] private PlayerMovement playerMovement; 
    [SerializeField] private float normalSpeed = 2f;    // Menyesuaikan default dari script PlayerMovement Anda
    [SerializeField] private float unarmedSpeed = 4f;   // Speed saat bertangan kosong (lebih cepat)

    private WeaponData[] slots = new WeaponData[TOTAL_SLOTS];
    private int currentSlot = SLOT_UNARMED;
    private WeaponPickup nearestPickup;

    private PlayerHealth playerHealth;
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
        
        playerHealth = GetComponent<PlayerHealth>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        allChildrenCache = GetComponentsInChildren<Transform>(true);
        EquipSlot(SLOT_UNARMED);
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (playerHealth != null && playerHealth.IsDead)
        {
            if (nearestPickup != null)
            {
                nearestPickup = null;
                onNearestPickupChanged?.Invoke(null);
            }
            return;
        }

        HandlePickupDetection();
        HandlePickupInput();
        HandleDropInput();
        HandleSlotSwitch();
    }

    // =========================================================================
    // HELPER UNTUK MENENTUKAN KATEGORI SENJATA (ANTI-DUPLIKAT)
    // =========================================================================
    private string GetWeaponCategory(WeaponData data)
    {
        if (data == null || string.IsNullOrEmpty(data.weaponName)) return "Unknown";

        string nameLower = data.weaponName.ToLower();

        // Mengelompokkan semua varian pedang balon ke kategori "Sword"
        if (nameLower.Contains("sword") || nameLower.Contains("blade") || nameLower.Contains("calibur"))
            return "Sword";

        if (nameLower.Contains("cannon") || nameLower.Contains("blaster") || nameLower.Contains("artillery"))
            return "Cannon";

        if (nameLower.Contains("hammer") || nameLower.Contains("mallet"))
            return "Hammer";

        return "Unknown";
    }

    // =========================================================================
    // HELPER UNTUK MENENTUKAN TAMPILAN VISUAL UTAMA (SHARED VISUALS)
    // =========================================================================
    private string GetVisualWeaponName(string originalWeaponName)
    {
        string nameLower = originalWeaponName.ToLower();

        // 1. Semua varian Pedang Balon diarahkan ke model visual: balloonsword
        if (nameLower.Contains("balloonsword") || nameLower.Contains("baloonsword"))
            return "balloonsword";

        // 2. Semua varian Hand Cannon diarahkan ke model visual: handcannon
        if (nameLower.Contains("handcannon") || nameLower.Contains("handcannpn"))
            return "handcannon";

        // 3. Semua varian Palu diarahkan ke model visual: squeekhammer
        if (nameLower.Contains("toyhammer") || nameLower.Contains("squeekhammer"))
            return "squeekhammer";

        return nameLower;
    }

    public void TryPickup(WeaponPickup pickup)
    {
        WeaponData data = pickup.Data;
        
        // --- VALIDASI ANTI DUPLIKAT KATEGORI ---
        string newCategory = GetWeaponCategory(data);
        if (newCategory != "Unknown")
        {
            for (int i = SLOT_WEAPON_1; i <= SLOT_WEAPON_2; i++)
            {
                if (slots[i] != null && GetWeaponCategory(slots[i]) == newCategory)
                {
                    Debug.LogWarning($"[Inventory] Gagal! Kamu sudah memiliki senjata tipe {newCategory} ({slots[i].weaponName}) di inventory.");
                    return; // Mengunci pickup agar tidak mengeksekusi kode di bawahnya
                }
            }
        }

        int emptySlot = FindEmptyWeaponSlot();

        if (emptySlot != -1)
        {
            SetSlot(emptySlot, data);
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
        if (currentSlot == SLOT_UNARMED) return;
        if (slots[currentSlot] == null) return;

        DropFromSlot(currentSlot);
        SwitchToSlot(SLOT_UNARMED);
    }

    private void SetSlot(int index, WeaponData data)
    {
        slots[index] = data;
        onSlotChanged?.Invoke(index, data);
    }

    private void DropFromSlot(int index)
    {
        WeaponData data = slots[index];
        if (data == null) return;

        if (data.pickupPrefab != null)
        {
            Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + transform.forward * 0.8f;
            if (dropPoint != null) spawnPos.y = dropPoint.position.y;

            GameObject dropped = Instantiate(data.pickupPrefab, spawnPos, Quaternion.identity);
            if (dropped.TryGetComponent<WeaponPickup>(out WeaponPickup wp))
                wp.MarkAsDropped();
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
        string activeWeaponName = "";
        bool isUnarmedActive = false; 

        if (index == SLOT_UNARMED && activeData == null)
        {
            activeWeaponName = "unarmed";
            isUnarmedActive = true;
        }
        else if (activeData != null)
        {
            // Mengubah ke huruf kecil dan membersihkan karakter spasi/underscore
            string rawName = activeData.weaponName.Replace("_", " ").Replace("  ", " ").ToLower();
            
            // DIALIKAN LEWAT HELPER: Mengarahkan nama varian ke nama visual induknya
            activeWeaponName = GetVisualWeaponName(rawName);
            
            if (activeWeaponName == "unarmed") isUnarmedActive = true;
        }

        // --- MANAJEMEN SPEED DINAMIS ---
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = isUnarmedActive ? unarmedSpeed : normalSpeed; 
        }

        WeaponHitbox foundHitbox = null;
        Weapon foundWeaponComponent = null; 

        foreach (Transform child in allChildrenCache)
        {
            if (child == null || child == transform) continue;
            string childName = child.gameObject.name.Replace("_", " ").Replace("  ", " ").ToLower();

            if (childName.EndsWith("hand"))
            {
                string weaponName = childName.Substring(0, childName.Length - 4).Trim();
                bool isWeaponActive = !string.IsNullOrEmpty(activeWeaponName) && activeWeaponName == weaponName;

                child.gameObject.SetActive(isWeaponActive);

                if (isWeaponActive)
                {
                    foundHitbox = child.GetComponent<WeaponHitbox>();
                    if (foundHitbox == null) foundHitbox = child.GetComponentInChildren<WeaponHitbox>(true);

                    Weapon[] weapons = child.GetComponents<Weapon>();
                    foreach (Weapon w in weapons)
                    {
                        if (w is not WeaponHitbox) 
                        {
                            foundWeaponComponent = w;
                            break;
                        }
                    }

                    if (foundWeaponComponent == null)
                    {
                        Weapon[] childWeapons = child.GetComponentsInChildren<Weapon>(true);
                        foreach (Weapon w in childWeapons)
                        {
                            if (w is not WeaponHitbox)
                            {
                                foundWeaponComponent = w;
                                break;
                            }
                        }
                    }
                }
                continue;
            }

            if (childName.EndsWith("back"))
            {
                string weaponName = childName.Substring(0, childName.Length - 4).Trim();
                
                // Pengecekan kepemilikan di punggung juga diarahkan ke pencocokan nama visual dasar
                bool isOwnedInSlot1 = slots[SLOT_WEAPON_1] != null && GetVisualWeaponName(slots[SLOT_WEAPON_1].weaponName) == weaponName;
                bool isOwnedInSlot2 = slots[SLOT_WEAPON_2] != null && GetVisualWeaponName(slots[SLOT_WEAPON_2].weaponName) == weaponName;
                
                bool isWeaponOwned = isOwnedInSlot1 || isOwnedInSlot2;
                bool isWeaponActive = !string.IsNullOrEmpty(activeWeaponName) && activeWeaponName == weaponName;
                bool shouldShowOnBack = isWeaponOwned && !isWeaponActive;

                child.gameObject.SetActive(shouldShowOnBack);
                continue;
            }
        }

        if (playerAttack != null)
        {
            if (playerAttack.GetActiveWeapon() != null)
            {
                playerAttack.GetActiveWeapon().OnWeaponDeactivate();
            }

            playerAttack.SetWeaponHitbox(foundHitbox);

            if (index == SLOT_UNARMED && activeData == null)
            {
                WeaponData fakeUnarmedData = ScriptableObject.CreateInstance<WeaponData>();
                fakeUnarmedData.weaponName = "Unarmed";
                fakeUnarmedData.damage = 15;             
                fakeUnarmedData.attackDuration = 0.2f;    
                fakeUnarmedData.attackCooldown = 0.25f;   
                
                playerAttack.EquipWeapon(fakeUnarmedData, foundWeaponComponent);
            }
            else
            {
                playerAttack.EquipWeapon(activeData, foundWeaponComponent);
            }

            if (foundWeaponComponent != null)
            {
                foundWeaponComponent.OnWeaponActivate();
            }
        }

        if (index == SLOT_UNARMED && activeData == null)
    {
        Debug.Log("<color=white>[Weapon Check] Slot saat ini: Bertangan Kosong (Unarmed)</color>");
    }
    else if (activeData != null)
    {
        Debug.Log($"<color=yellow>[Weapon Check] Slot saat ini: {activeData.weaponName} | Damage: {activeData.damage} | Cooldown: {activeData.attackCooldown}</color>");
    }
    }

    private void HandleSlotSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToSlot(2);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)  SwitchToSlot((currentSlot + 1) % TOTAL_SLOTS);
        else if (scroll < 0f) SwitchToSlot((currentSlot - 1 + TOTAL_SLOTS) % TOTAL_SLOTS);
    }

    private void HandleDropInput()
    {
        if (Input.GetKeyDown(KeyCode.G)) TryDrop();
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
        if (Input.GetKeyDown(KeyCode.F) && nearestPickup != null) TryPickup(nearestPickup);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}