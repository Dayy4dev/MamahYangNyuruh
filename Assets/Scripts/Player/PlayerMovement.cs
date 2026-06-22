using UnityEngine;

[AddComponentMenu("Player/Player Movement")]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Kecepatan gerak player")]
    public float moveSpeed = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Weapon Inventory")]
    public int startingWeaponIndex = 0;
    public GameObject weaponParent;

    private CharacterController controller;
    private Animator animator;
    private Weapon[] weapons;
    private Weapon equippedWeapon;
    private int currentWeaponIndex;

    private Vector3 velocity;
    private bool isGrounded;

    // Animator parameter hashes (lebih efisien dari string)
    private static readonly int AnimHorizontal = Animator.StringToHash("Horizontal");
    private static readonly int AnimVertical   = Animator.StringToHash("Vertical");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator   = GetComponentInChildren<Animator>();

        if (weaponParent != null)
            weapons = weaponParent.GetComponentsInChildren<Weapon>(true);

        if (weapons != null && weapons.Length > 0)
            EquipWeapon(Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1));
        else
            Debug.LogWarning("[PlayerMovement] Weapons array kosong! Pastikan weaponParent sudah di-assign.");
    }

    void Update()
    {
        if (controller == null) return;

        HandleGravity();
        HandleMovement();
        HandleRotation();
        HandleWeaponSwitch();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void AimTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (!groundPlane.Raycast(ray, out float distance)) return;

        Vector3 mouseWorldPos = ray.GetPoint(distance);
        Vector3 direction = new Vector3(
            mouseWorldPos.x - transform.position.x,
            0f,
            mouseWorldPos.z - transform.position.z
        );

        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }

    // -------------------------------------------------------------------------
    // Private — Movement
    // -------------------------------------------------------------------------

    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f; // Nilai kecil negatif agar isGrounded tetap stabil

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(x, 0f, z).normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (animator == null) return;

        Vector3 localMove = transform.InverseTransformDirection(move);
        float smoothSpeed = Time.deltaTime * 5f;
        animator.SetFloat(AnimHorizontal, Mathf.MoveTowards(animator.GetFloat(AnimHorizontal), localMove.x, smoothSpeed));
        animator.SetFloat(AnimVertical,   Mathf.MoveTowards(animator.GetFloat(AnimVertical),   localMove.z, smoothSpeed));
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            AimTowardsMouse();

            if (Input.GetKeyDown(KeyCode.Mouse0))
                equippedWeapon?.Attack();
        }
        else
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");
            Vector3 move = new Vector3(x, 0f, z).normalized;

            if (move != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Private — Weapon Switch
    // -------------------------------------------------------------------------

    private void HandleWeaponSwitch()
    {
        if (weapons == null || weapons.Length == 0) return;

        // Hotkey 1–9
        for (int i = 0; i < weapons.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                EquipWeapon(i);
        }

        // Scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            EquipWeapon((currentWeaponIndex + 1) % weapons.Length);
        else if (scroll < 0f)
            EquipWeapon((currentWeaponIndex - 1 + weapons.Length) % weapons.Length);
    }

    private void EquipWeapon(int index)
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("[PlayerMovement] Weapons array kosong!");
            return;
        }

        if (index < 0 || index >= weapons.Length) return;

        // Nonaktifkan semua senjata
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                Debug.LogWarning($"[PlayerMovement] Weapon index {i} null!");
                continue;
            }

            weapons[i].OnWeaponDeactivate();
            weapons[i].gameObject.SetActive(false);

            if (weapons[i].weaponRig != null)
                weapons[i].weaponRig.weight = 0f;
        }

        currentWeaponIndex = index;

        Weapon selected = weapons[currentWeaponIndex];
        if (selected == null)
        {
            Debug.LogWarning($"[PlayerMovement] Weapon index {currentWeaponIndex} null!");
            return;
        }

        selected.gameObject.SetActive(true);
        equippedWeapon = selected;
        equippedWeapon.OnWeaponActivate();

        if (equippedWeapon.weaponRig != null)
            equippedWeapon.weaponRig.weight = 1f;
    }

    // -------------------------------------------------------------------------
    // Trigger — Wall Fader
    // -------------------------------------------------------------------------

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<WallFader>(out WallFader wall))
            wall.FadeToTransparent();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<WallFader>(out WallFader wall))
            wall.FadeToOpaque();
    }
}