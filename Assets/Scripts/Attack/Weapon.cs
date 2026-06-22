using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class Weapon : MonoBehaviour
{
    [Header("Animation Rigging")]
    public Rig weaponRig;

    public abstract void Attack();

    public virtual void OnWeaponActivate() { }

    public virtual void OnWeaponDeactivate() { }
}