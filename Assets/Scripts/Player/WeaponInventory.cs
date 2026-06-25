using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public static WeaponInventory Instance { get; private set; }

    [Header("Equipped Weapon Slots")]
    public WeaponData primaryWeapon;
    public WeaponData secondaryWeapon;

    [Header("Inventory Pool")]
    public List<WeaponData> ownedWeapons = new List<WeaponData>();

    private void Awake()
    {
        // This registers the Game Manager's component as the global instance
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    public void AddWeaponToPool(WeaponData weapon)
    {
        if (weapon != null && !ownedWeapons.Contains(weapon))
        {
            ownedWeapons.Add(weapon);
        }
    }
}