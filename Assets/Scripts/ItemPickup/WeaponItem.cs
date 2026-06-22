using UnityEngine;

[AddComponentMenu("Items/Weapon Item")]
public class WeaponItem : ItemPickup
{
    [Header("Weapon")]
    [SerializeField] private Weapon weapon;

    protected override void Awake()
    {
        base.Awake();

        // Get the Weapon component on this object if not assigned
        if (weapon == null)
            weapon = GetComponent<Weapon>();

        if (weapon == null)
            Debug.LogWarning("[WeaponItem] No Weapon component found!");
    }

    protected override void OnPickup(GameObject player)
    {
        if (weapon == null) return;

        weapon.SetPickedUp(true);

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.AddWeapon(weapon.gameObject);
            Debug.Log($"[WeaponItem] Added weapon '{weapon.gameObject.name}' to inventory!");
        }
        else
        {
            Debug.LogWarning("[WeaponItem] Player doesn't have PlayerMovement component!");
        }
    }
}
