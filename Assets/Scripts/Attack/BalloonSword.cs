using UnityEngine;
using System.Collections;

public class BalloonSword : Weapon
{
    private WeaponHitbox hitbox;
    [SerializeField] private int damage = 25;
    [SerializeField] private WeaponData weaponData;

    // State Melee Reload/Rehat
    private int maxComboCount;
    private float meleeReloadTime;
    private int currentComboLeft;
    private bool isReheating;
    private float remainingReheatTime;
    private Coroutine reheatCoroutine;

    private void Awake()
    {
        hitbox = GetComponent<WeaponHitbox>();
        if (hitbox == null)
        {
            Debug.LogWarning("[BalloonSword] No WeaponHitbox component found!");
        }
    }

    private void Start()
    {
        if (weaponData != null)
        {
            maxComboCount = weaponData.maxComboCount;
            meleeReloadTime = weaponData.meleeReloadTime;
        }
        currentComboLeft = maxComboCount;
    }

    private void OnDisable()
    {
        if (reheatCoroutine != null)
        {
            StopCoroutine(reheatCoroutine);
            reheatCoroutine = null;
        }
        isReheating = false;
        remainingReheatTime = 0f;
    }

    public override bool CanAttack()
    {
        return !isReheating && currentComboLeft > 0;
    }

    public override void Attack()
    {
        if (!CanAttack() || hitbox == null) return;

        hitbox.Activate(damage);
        Invoke(nameof(DeactivateHitbox), 0.2f);

        // Kurangi jatah ayunan
        currentComboLeft--;

        if (currentComboLeft <= 0)
        {
            reheatCoroutine = StartCoroutine(ReheatCoroutine());
        }
    }

    private void DeactivateHitbox()
    {
        if (hitbox != null)
            hitbox.Deactivate();
    }

    public override void OnWeaponDeactivate()
    {
        if (isReheating && reheatCoroutine != null)
        {
            StopCoroutine(reheatCoroutine);
            reheatCoroutine = null;
            Debug.Log($"[BalloonSword] Rehat dipause. Sisa: {remainingReheatTime:F1}s");
        }
    }

    public override void OnWeaponActivate()
    {
        if (currentComboLeft <= 0 && !isReheating)
        {
            reheatCoroutine = StartCoroutine(ReheatCoroutine());
            return;
        }

        if (isReheating && reheatCoroutine == null && remainingReheatTime > 0)
        {
            reheatCoroutine = StartCoroutine(ReheatWithRemainingTime(remainingReheatTime));
            Debug.Log($"[BalloonSword] Rehat dilanjutkan. Sisa: {remainingReheatTime:F1}s");
        }
    }

    private IEnumerator ReheatCoroutine()
    {
        isReheating = true;
        remainingReheatTime = meleeReloadTime;
        Debug.Log("[BalloonSword] Senjata lelah, sedang rehat...");

        while (remainingReheatTime > 0f)
        {
            remainingReheatTime -= Time.deltaTime;
            yield return null;
        }

        CompleteReheat();
    }

    private IEnumerator ReheatWithRemainingTime(float remainingTime)
    {
        isReheating = true;
        remainingReheatTime = remainingTime;

        while (remainingReheatTime > 0f)
        {
            remainingReheatTime -= Time.deltaTime;
            yield return null;
        }

        CompleteReheat();
    }

    private void CompleteReheat()
    {
        currentComboLeft = maxComboCount;
        isReheating = false;
        reheatCoroutine = null;
        remainingReheatTime = 0f;
        Debug.Log("[BalloonSword] Rehat selesai! Siap menebas lagi.");
    }

    public override float GetCooldownPercentage()
    {
        if (isReheating && meleeReloadTime > 0f)
        {
            return remainingReheatTime / meleeReloadTime;
        }
        return 0f;
    }
}