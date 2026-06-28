using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInventory playerInventory;
    
    [Header("Slot References (always 3 slots)")]
    [SerializeField] private WeaponSlotUI unarmedSlot;
    [SerializeField] private WeaponSlotUI primarySlot;
    [SerializeField] private WeaponSlotUI secondarySlot;
    
    [Header("Active Visuals")]
    [SerializeField] private Image unarmedBorder;
    [SerializeField] private Image primaryBorder;
    [SerializeField] private Image secondaryBorder;
    [SerializeField] private Color activeColor = Color.pink;
    [SerializeField] private Color inactiveColor = Color.white;

    private void Start()
    {
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();

        if (playerInventory != null)
        {
            playerInventory.onSlotChanged.AddListener(OnSlotChanged);
            playerInventory.onActiveSlotChanged.AddListener(OnActiveSlotChanged);
            
            RefreshAllSlots();
            OnActiveSlotChanged(playerInventory.CurrentSlot);
        }
    }

    public void RefreshAllSlots()
    {
        if (playerInventory == null) return;

        if (primarySlot != null)
            primarySlot.SetWeapon(playerInventory.Slots[PlayerInventory.SLOT_WEAPON_1]);
        if (secondarySlot != null)
            secondarySlot.SetWeapon(playerInventory.Slots[PlayerInventory.SLOT_WEAPON_2]);

        if (unarmedBorder != null) unarmedBorder.gameObject.SetActive(true);
        if (primaryBorder != null) primaryBorder.gameObject.SetActive(true);
        if (secondaryBorder != null) secondaryBorder.gameObject.SetActive(true);
    }
    
    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.onSlotChanged.RemoveListener(OnSlotChanged);
            playerInventory.onActiveSlotChanged.RemoveListener(OnActiveSlotChanged);
        }
    }

    private void OnSlotChanged(int slotIndex, WeaponData data)
    {
        if (slotIndex == PlayerInventory.SLOT_WEAPON_1 && primarySlot != null)
        {
            primarySlot.SetWeapon(data);
        }
        else if (slotIndex == PlayerInventory.SLOT_WEAPON_2 && secondarySlot != null)
        {
            secondarySlot.SetWeapon(data);
        }
    }
    
    private void OnActiveSlotChanged(int activeSlotIndex)
    {
        if (unarmedBorder != null)
            unarmedBorder.color = (activeSlotIndex == PlayerInventory.SLOT_UNARMED) ? activeColor : inactiveColor;
        if (primaryBorder != null)
            primaryBorder.color = (activeSlotIndex == PlayerInventory.SLOT_WEAPON_1) ? activeColor : inactiveColor;
        if (secondaryBorder != null)
            secondaryBorder.color = (activeSlotIndex == PlayerInventory.SLOT_WEAPON_2) ? activeColor : inactiveColor;
    }
}
