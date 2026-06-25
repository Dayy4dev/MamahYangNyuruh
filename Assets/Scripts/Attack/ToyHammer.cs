using UnityEngine;

public class ToyHammer : Weapon
{
    private WeaponHitbox hitbox;
    [SerializeField] private int damage = 35;

    private void Awake()
    {
        hitbox = GetComponent<WeaponHitbox>();
        if (hitbox == null)
        {
            Debug.LogWarning("[ToyHammer] No WeaponHitbox component found!");
        }
    }

    public override void Attack()
    {
        if (hitbox != null)
        {
            hitbox.Activate(damage);
            Invoke(nameof(DeactivateHitbox), 0.3f);
        }
    }

    private void DeactivateHitbox()
    {
        if (hitbox != null)
            hitbox.Deactivate();
    }
}
