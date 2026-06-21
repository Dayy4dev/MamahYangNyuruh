using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private WeaponSlotUI primarySlot;
    [SerializeField] private WeaponSlotUI secondarySlot;

    public void Refresh()
    {
        // Safe guard in case managers are initializing
        if (WeaponInventory.Instance == null) return;
        if (primarySlot == null || secondarySlot == null) return;

        primarySlot.SetWeapon(WeaponInventory.Instance.primaryWeapon);
        secondarySlot.SetWeapon(WeaponInventory.Instance.secondaryWeapon);
    }
}