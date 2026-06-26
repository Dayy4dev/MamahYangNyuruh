using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttack : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("References")]
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private AudioSource audioSource; // Komponen pemutar suara Player

    [Header("Audio Tracks (Suara Ayunan / Tembakan)")]
    [SerializeField] private AudioClip[] swordAttackCycles; // Masukkan 3 clip suara ayunan pedang (Element 0, 1, 2)
    [SerializeField] private AudioClip handCannonSound;     // Suara tembakan meriam tembak
    [SerializeField] private AudioClip squeekHammerSound;   // Suara palu mainan melengking

    [Header("Audio Tracks (Suara Pasang / Equip)")]
    [SerializeField] private AudioClip genericMeleeEquipSound; // Suara cabut pedang / angkat palu
    [SerializeField] private AudioClip handCannonEquipSound;   // Suara kokang / jepretan HandCannon

    [Header("Ranged Setup ")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform firePoint; 
    private ObjectPool<Bullet> bulletPool;   

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private WeaponData currentWeaponData;
    private Weapon activeWeapon; 
    private int comboStep = 0; // Melacak index ayunan pedang (0 -> 1 -> 2)

    private int   attackDamage;
    private float attackDuration;
    private float attackCooldown;
    private float knockbackForce;
    private float knockbackDuration;

    private float attackTimer;
    private float cooldownTimer;
    private bool  isAttacking;
    private bool  isArmed; 

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------


    private void Awake()
    {
        bulletPool = new ObjectPool<Bullet>(
            createFunc: CreateBullet,
            actionOnGet: OnBulletGet,
            actionOnRelease: OnBulletRelease,
            actionOnDestroy: OnBulletDestroy,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50        
        );
    }

    // Fungsi Callback untuk Pool
    private Bullet CreateBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab);
        bulletObj.SetActive(false);
        return bulletObj.GetComponent<Bullet>();
    }

    private void OnBulletGet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void OnBulletRelease(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnBulletDestroy(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }

    void Update()
    {
        TickAttack();
        TickCooldown();
        HandleInput();
    }

    
    public void EquipWeapon(WeaponData data, Weapon weaponComponent)
    {
        EndAttack(); 

        currentWeaponData = data;
        activeWeapon = weaponComponent;
        isArmed = data != null;
        comboStep = 0; 

        if (isArmed)
        {
            attackDamage      = data.damage;
            attackDuration    = data.attackDuration;
            attackCooldown    = data.attackCooldown;
            knockbackForce    = data.knockbackForce;
            knockbackDuration = data.knockbackDuration;
            
            if (audioSource != null)
            {
                if (data.weaponName.Contains("Hand_Cannon") || data.weaponName.Contains("HandCannon"))
                {
                    if (handCannonEquipSound != null) audioSource.PlayOneShot(handCannonEquipSound);
                }
                else
                {
                    if (genericMeleeEquipSound != null) audioSource.PlayOneShot(genericMeleeEquipSound);
                }
            }
        }
    }

    public void SetWeaponHitbox(WeaponHitbox newHitbox)
    {
        this.weaponHitbox = newHitbox;
    }

    public void ApplyKnockback(GameObject target)
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;
        dir.y = 0f;

        if (target.TryGetComponent<IKnockbackable>(out IKnockbackable kb))
            kb.TakeKnockback(dir, knockbackForce, knockbackDuration);
        else if (target.TryGetComponent<Rigidbody>(out Rigidbody rb))
            rb.AddForce(dir * knockbackForce, ForceMode.Impulse);
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void HandleInput()
    {
        if (!isArmed) return;

        // Tahan klik kanan untuk aim, klik kiri untuk attack
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0))
        {
            if (!isAttacking && cooldownTimer <= 0f)
                StartAttack();
        }
    }

    private void TickAttack()
    {
        if (!isAttacking) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
            EndAttack();
    }

    private void TickCooldown()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void StartAttack()
    {
        isAttacking   = true;
        attackTimer   = attackDuration;
        cooldownTimer = attackCooldown;

        if (currentWeaponData != null)
        {
            if (currentWeaponData.weaponName == "Hand_Cannon" || currentWeaponData.weaponName == "HandCannon")
            {
                HandCannon handCannon = activeWeapon as HandCannon;
                if (handCannon != null && !handCannon.CanFire())
                {
                    isAttacking = false;
                    return;
                }

                if (audioSource != null && handCannonSound != null) 
                    audioSource.PlayOneShot(handCannonSound);

                FireRangedWeapon();

                if (handCannon != null)
                    handCannon.ConsumeBullet();

                cooldownTimer = currentWeaponData.fireRate;
            }
            else if (currentWeaponData.weaponName == "SqueekHammer" || currentWeaponData.weaponName == "ToyHammer")
            {
                if (audioSource != null && squeekHammerSound != null) 
                    audioSource.PlayOneShot(squeekHammerSound);

                TriggerMeleeHitbox();
            }
            else
            {
                PlaySwordComboSound();
                TriggerMeleeHitbox();
            }
        }
    }

    private void PlaySwordComboSound()
    {
        if (swordAttackCycles == null || swordAttackCycles.Length == 0) return;

        if (audioSource != null && swordAttackCycles[comboStep] != null)
        {
            audioSource.PlayOneShot(swordAttackCycles[comboStep]);
        }

        comboStep = (comboStep + 1) % swordAttackCycles.Length;
    }

    private void TriggerMeleeHitbox()
    {
        if (weaponHitbox == null)
        {
            Debug.LogWarning("[PlayerAttack] WeaponHitbox belum terpasang untuk senjata Melee!");
            return;
        }
        weaponHitbox.Activate(attackDamage);
    }

    private void FireRangedWeapon()
{
    if (bulletPrefab == null)
    {
        Debug.LogError("[PlayerAttack] Bullet Prefab belum dimasukkan di Inspector!");
        return;
    }

    Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 1f;
    Quaternion spawnRot = transform.rotation;

    Bullet bullet = bulletPool.Get();
    
    bullet.transform.SetPositionAndRotation(spawnPos, spawnRot);
    
    bullet.SetPool(bulletPool);
    
    bullet.Setup(currentWeaponData.speed, currentWeaponData.damage);

    if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
    {
        rb.linearVelocity = Vector3.zero; 
    }
    
    Debug.Log($"[PlayerAttack] Menembakkan {currentWeaponData.weaponName}!");
}

    private void EndAttack()
    {
        isAttacking = false;
        weaponHitbox?.Deactivate();
    }
}