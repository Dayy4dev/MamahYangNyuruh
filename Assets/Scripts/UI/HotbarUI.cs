using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerInventory playerInventory;
    
    [Header("Slot References")]
    [SerializeField] private WeaponSlotUI primarySlot;
    [SerializeField] private WeaponSlotUI secondarySlot;
    
    [Header("Active Visuals")]
    [SerializeField] private Image primaryBorder;
    [SerializeField] private Image secondaryBorder;
    [SerializeField] private Color activeColor = Color.pink;
    [SerializeField] private Color inactiveColor = Color.white;

    private void Start()
    {
        // Use FindFirstObjectByType to be compatible with newer Unity versions (or FindObjectOfType for older ones)
        if (playerInventory == null)
            playerInventory = FindFirstObjectByType<PlayerInventory>();

        if (playerInventory != null)
        {
            playerInventory.onSlotChanged.AddListener(OnSlotChanged);
            playerInventory.onActiveSlotChanged.AddListener(OnActiveSlotChanged);
            
            // Initialize visuals with current data
            OnSlotChanged(PlayerInventory.SLOT_WEAPON_1, playerInventory.Slots[PlayerInventory.SLOT_WEAPON_1]);
            OnSlotChanged(PlayerInventory.SLOT_WEAPON_2, playerInventory.Slots[PlayerInventory.SLOT_WEAPON_2]);
            OnActiveSlotChanged(playerInventory.CurrentSlot);
        }
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
        if (slotIndex == PlayerInventory.SLOT_WEAPON_1) 
        {
            if (primarySlot != null) primarySlot.SetWeapon(data);
            if (primaryBorder != null) primaryBorder.gameObject.SetActive(data != null);
        }
        else if (slotIndex == PlayerInventory.SLOT_WEAPON_2) 
        {
            if (secondarySlot != null) secondarySlot.SetWeapon(data);
            if (secondaryBorder != null) secondaryBorder.gameObject.SetActive(data != null);
        }
    }
    
    private void OnActiveSlotChanged(int activeSlotIndex)
    {
        if (primaryBorder != null) 
            primaryBorder.color = (activeSlotIndex == PlayerInventory.SLOT_WEAPON_1) ? activeColor : inactiveColor;
            
        if (secondaryBorder != null) 
            secondaryBorder.color = (activeSlotIndex == PlayerInventory.SLOT_WEAPON_2) ? activeColor : inactiveColor;
    }
}
