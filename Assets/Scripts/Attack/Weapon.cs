using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class Weapon : MonoBehaviour
{
    [Header("Animation Rigging")]
    public Rig weaponRig;

    [SerializeField]private bool pickedUp = false; // Default to true for weapons already in scene

    public abstract void Attack();

    public virtual void OnWeaponActivate() { }

    public virtual void OnWeaponDeactivate() { }

    public virtual void OnWeaponActivate() { }

    public void SetPickedUp(bool value)
    {
        pickedUp = value;
    }

    public bool IsPickedUp()
    {
        return pickedUp;
    }
}