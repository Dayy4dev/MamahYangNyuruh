using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Base class semua senjata.
/// IsPickedUp() dipertahankan untuk kompatibilitas,
/// tapi logika inventory kini dihandle PlayerInventory.
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [Header("Animation Rigging")]
    public Rig weaponRig;

    public abstract void Attack();

    public virtual void OnWeaponActivate()   { }
    public virtual void OnWeaponDeactivate() { }
}