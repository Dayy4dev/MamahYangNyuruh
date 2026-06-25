using UnityEngine;

// Klik kanan di Project → Create → Weapons → Weapon Data
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName = "Unnamed Weapon";
    public Sprite icon;

    [Header("Pickup")]
    [Tooltip("Prefab WeaponPickup yang di-spawn saat senjata dijatuhkan")]
    public GameObject pickupPrefab;

    [Header("Melee Stats")]
    public int   damage           = 10;
    public float attackDuration   = 0.2f;
    public float attackCooldown   = 0.5f;
    public float knockbackForce   = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Ranged Stats")]
    public int   defaultCapacity = 0;
    public int   maxPoolSize     = 0;
    public float speed           = 0f;
    public int   magazineSize    = 0;
    public float reloadTime      = 0f;
}