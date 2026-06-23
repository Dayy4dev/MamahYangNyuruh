using UnityEngine;
using TMPro;

/// <summary>
/// Menampilkan / menyembunyikan prompt "[F] Ambil <nama senjata>"
/// saat player berada dalam jangkauan WeaponPickup.
///
/// Setup:
/// 1. Buat Canvas (Screen Space - Overlay) di scene
/// 2. Tambahkan GameObject → TextMeshPro di dalam Canvas
/// 3. Assign komponen ini ke Player, hubungkan promptText di Inspector
/// 4. Hubungkan PlayerInventory.onNearestPickupChanged ke OnNearestPickupChanged()
/// </summary>
public class PickupPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<PlayerInventory>();

        HidePrompt();
    }

    void OnEnable()
    {
        if (inventory != null)
            inventory.onNearestPickupChanged.AddListener(OnNearestPickupChanged);
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.onNearestPickupChanged.RemoveListener(OnNearestPickupChanged);
    }

    // -------------------------------------------------------------------------
    // Listener
    // -------------------------------------------------------------------------

    private void OnNearestPickupChanged(WeaponPickup pickup)
    {
        if (pickup == null || pickup.Data == null)
        {
            HidePrompt();
            return;
        }

        string action = IsInventoryFull() ? "Tukar" : "Ambil";
        promptText.text = $"[F] {action}: {pickup.Data.weaponName}";
        promptPanel.SetActive(true);
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void HidePrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);
    }

    private bool IsInventoryFull()
    {
        if (inventory == null) return false;
        WeaponData[] slots = inventory.Slots;
        return slots[PlayerInventory.SLOT_WEAPON_1] != null &&
               slots[PlayerInventory.SLOT_WEAPON_2] != null;
    }
}