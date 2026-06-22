using UnityEngine;

/// <summary>
/// Komponen yang ditempel di prefab senjata yang ada di dunia.
/// Saat player mendekat → tampilkan prompt "[F] Ambil <nama>"
/// Saat F ditekan → PlayerInventory.TryPickup(this) dipanggil
/// Saat di-drop → ditandai sebagai dropped, dihapus saat scene unload
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Data")]
    [SerializeField] private WeaponData weaponData;

    [Header("Animation")]
    [SerializeField] private bool enableFloat    = true;
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed     = 2f;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private float startY;
    private bool isDropped; // true = spawn dari drop player, dihapus saat scene unload

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public WeaponData Data => weaponData;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Awake()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        if (!enableFloat) return;

        transform.Rotate(rotationSpeed * Time.deltaTime);
        float newY = startY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnEnable()
    {
        startY = transform.position.y;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Dipanggil PlayerInventory setelah senjata berhasil diambil.</summary>
    public void OnPickedUp()
    {
        Destroy(gameObject);
    }

    /// <summary>Tandai sebagai senjata yang dijatuhkan player (dihapus saat pindah scene).</summary>
    public void MarkAsDropped()
    {
        isDropped = true;
        // Tidak DontDestroyOnLoad → otomatis terhapus saat scene berganti
    }

    public bool IsDropped => isDropped;
}