using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [Header("UI Component Child Bindings")]
    [SerializeField] private Image    weaponImage;
    [SerializeField] private TMP_Text weaponName;

    private void Awake()
    {
        // If no Image was wired in the Inspector, fall back to the Image on
        // this same GameObject (used by the Hotbar's single-Image slot objects).
        if (weaponImage == null)
            weaponImage = GetComponent<Image>();
    }

    public void SetWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            if (weaponImage != null)
            {
                weaponImage.sprite  = null;
                weaponImage.enabled = false;
            }
            if (weaponName != null) weaponName.text = "Empty";
            return;
        }

        if (weaponImage != null)
        {
            weaponImage.enabled = true;
            weaponImage.sprite  = weapon.icon;
        }

        if (weaponName != null)
            weaponName.text = weapon.weaponName;
    }
}