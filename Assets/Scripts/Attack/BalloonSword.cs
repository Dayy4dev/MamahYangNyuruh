using UnityEngine;

public class BalloonSword : Weapon
{
    private WeaponHitbox hitbox;
    [SerializeField] private int damage = 25;

    private void Awake()
    {
        hitbox = GetComponent<WeaponHitbox>();
        if (hitbox == null)
        {
            Debug.LogWarning("[BalloonSword] No WeaponHitbox component found!");
        }
    }

    public override void Attack()
    {
        if (hitbox != null)
        {
            hitbox.Activate(damage);
            Invoke(nameof(DeactivateHitbox), 0.2f);
        }
    }

    private void DeactivateHitbox()
    {
        if (hitbox != null)
            hitbox.Deactivate();
    }
}
