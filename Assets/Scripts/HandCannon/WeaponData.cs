using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{


    [Header("Weapon Stats")]
    public int damage = 0;

    [Header("Melee Weapon")]
    [Tooltip("float dalam detik")]
    public float attackDuration;
    [Tooltip("float dalam detik")]
    public float attackCooldown = 0;
    [Tooltip("float")]
    public float knockbackForce = 0f;
    [Tooltip("float dalam detik")]
    public float knockbackDuration = 0f;

    [Header("Range Weapon")]
    [Tooltip("Int jumlah peluru default dalam magazin")]
    [SerializeField] public int defaultCapacity = 0;
    [Tooltip("Int jumlah peluru maksimal dalam pool")]
    [SerializeField] public int maxPoolSize = 0;
    [Tooltip("float")]
    [SerializeField] public float speed = 0;
    [Tooltip("Int jumlah peluru dalam magazin")]
    [SerializeField] public int magazineSize = 0;
    [Tooltip("float dalam detik")]
    [SerializeField] public float reloadTime = 0;

}