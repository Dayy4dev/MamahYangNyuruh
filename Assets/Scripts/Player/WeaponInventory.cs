using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public static WeaponInventory Instance { get; private set; }

    [Header("Owned Weapons")]
    public List<WeaponData> ownedWeapons = new();

    [Header("Equipped Weapons")]
    public WeaponData primaryWeapon;
    public WeaponData secondaryWeapon;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddWeapon(WeaponData weapon)
    {
        if (weapon != null && !ownedWeapons.Contains(weapon))
        {
            ownedWeapons.Add(weapon);
        }
    }

    public void EquipPrimary(WeaponData weapon)
    {
        if (weapon == null || ownedWeapons.Contains(weapon))
        {
            primaryWeapon = weapon;
        }
    }

    public void EquipSecondary(WeaponData weapon)
    {
        if (weapon == null || ownedWeapons.Contains(weapon))
        {
            secondaryWeapon = weapon;
        }
    }
}