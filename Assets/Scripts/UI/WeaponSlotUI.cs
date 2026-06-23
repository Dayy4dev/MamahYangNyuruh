using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("UI Component Child Bindings")]
    [SerializeField] private Image weaponImage;
    [SerializeField] private TMP_Text weaponName;

    public void SetWeapon(WeaponData weapon)
    {
        // If no weapon data is assigned to this slot
        if (weapon == null)
        {
            if (weaponImage != null) weaponImage.enabled = false; // Hides the icon
            if (weaponName != null) weaponName.text = "Empty";
            return;
        }

        // If a weapon exists, load its asset data
        if (weaponImage != null)
        {
            weaponImage.enabled = true; // Shows the icon
            weaponImage.sprite = weapon.weaponIcon;
        }
        
        if (weaponName != null)
        {
            weaponName.text = weapon.weaponName;
        }
    }
}