using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [SerializeField] private Image weaponImage;
    [SerializeField] private TMP_Text weaponName;

    public void SetWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            if (weaponImage != null) weaponImage.enabled = false;
            if (weaponName != null) weaponName.text = "Empty";
            return;
        }

        if (weaponImage != null)
        {
            weaponImage.enabled = true;
            weaponImage.sprite = weapon.weaponIcon;
        }
        
        if (weaponName != null)
        {
            weaponName.text = weapon.weaponName;
        }
    }
}