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

    // --- TAMBAHKAN INI AGAR PLAYERATTACK BISA MENGECEK STATUS REHAT MELEE ---
    public virtual bool CanAttack() { return true; }

    public virtual void OnWeaponActivate() { }
    public virtual void OnWeaponDeactivate() { }

    // Mengembalikan nilai antara 0f (siap pakai) sampai 1f (sedang rehat penuh)
    public virtual float GetCooldownPercentage()
    {
        return 0f;
    }
}
