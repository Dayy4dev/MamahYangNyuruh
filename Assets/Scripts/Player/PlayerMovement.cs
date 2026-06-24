using UnityEngine;

[AddComponentMenu("Player/Player Movement")]
public class PlayerMovement : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Header("Movement")]
    [Tooltip("Kecepatan gerak player")]
    public float moveSpeed = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    // --- TAMBAHAN BARU UNTUK INDIKATOR AIM ---
    [Header("Aim Indicator")]
    [Tooltip("Masukkan GameObject Lingkaran Merah di sini")]
    public Transform aimIndicator;
    // -----------------------------------------

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;
    public Vector3 GetMouseTargetPosition { get; private set; }

    // Animator parameter hashes (lebih efisien dari string)
    private static readonly int AnimHorizontal = Animator.StringToHash("Horizontal");
    private static readonly int AnimVertical = Animator.StringToHash("Vertical");

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // --- TAMBAHAN BARU: Sembunyikan kursor OS & kunci di dalam window game ---
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        // -----------------------------------------------------------------------
    }

    void Update()
    {
        if (controller == null) return;

        HandleGravity();
        HandleMovement();
        HandleRotation();
        UpdateAimIndicatorPosition(); // --- TAMBAHAN BARU: Selalu update posisi lingkaran merah ---
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void AimTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);

            // Simpan posisi target untuk digunakan script senjata
            GetMouseTargetPosition = mouseWorldPos;

            Vector3 direction = new Vector3(
                mouseWorldPos.x - transform.position.x,
                0f,
                mouseWorldPos.z - transform.position.z
            );

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }
    }

    // -------------------------------------------------------------------------
    // Private — Movement & Aim
    // -------------------------------------------------------------------------

    // --- FUNGSI TAMBAHAN BARU: Mengatur posisi lingkaran merah di atas lantai ---
    private void UpdateAimIndicatorPosition()
    {
        if (aimIndicator == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);

            // 1. Update posisi (Y dinaikkan sedikit agar tidak berkedip/Z-fighting dengan lantai)
            aimIndicator.position = new Vector3(mouseWorldPos.x, transform.position.y + 0.02f, mouseWorldPos.z);

            // 2. PERBAIKAN ROTASI: Putar di sumbu Z lokal menggunakan Space.Self
            aimIndicator.Rotate(Vector3.forward, 50f * Time.deltaTime, Space.Self);
        }
    }

    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

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
        animator.SetFloat(AnimVertical, Mathf.MoveTowards(animator.GetFloat(AnimVertical), localMove.z, smoothSpeed));
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            // Klik kanan → aim ke mouse, attack dihandle PlayerAttack
            AimTowardsMouse();
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