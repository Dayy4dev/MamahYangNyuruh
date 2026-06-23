using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [Header("Slot References")]
    [SerializeField] private WeaponSlotUI primarySlot;
    [SerializeField] private WeaponSlotUI secondarySlot;

    private void Start()
    {
        // Force a graphic refresh on boot up as a safety net
        Invoke(nameof(RefreshUI), 0.1f);
    }

    public void RefreshUI()
    {
        // Verify the manager instance exists in the scene
        if (WeaponInventory.Instance == null) return;

        // Push data to the individual slots
        if (primarySlot != null)
        {
            primarySlot.SetWeapon(WeaponInventory.Instance.primaryWeapon);
        }

        if (secondarySlot != null)
        {
            secondarySlot.SetWeapon(WeaponInventory.Instance.secondaryWeapon);
        }
    }
}