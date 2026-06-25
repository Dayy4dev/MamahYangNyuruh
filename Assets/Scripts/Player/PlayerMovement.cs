using UnityEngine;

[AddComponentMenu("Player/Player Movement")]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Kecepatan gerak player")]
    public float moveSpeed = 2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Aim Indicator")]
    [Tooltip("Masukkan GameObject Lingkaran Merah di sini")]
    public Transform aimIndicator;

    private CharacterController controller;
    private Animator animator;
    
    // Perubahan logika gabungan gerakan
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 verticalVelocity = Vector3.zero;
    private bool isGrounded;
    
    public Vector3 GetMouseTargetPosition { get; private set; }

    private static readonly int AnimHorizontal = Animator.StringToHash("Horizontal");
    private static readonly int AnimVertical = Animator.StringToHash("Vertical");

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (controller == null) return;

        // 1. Hitung Kalkulasi Logika Dasar (Tanpa memanggil controller.Move dulu)
        CalculateGravity();
        CalculateMovementInput();
        
        // 2. Kalkulasi Raycast Mouse (Cukup 1x di sini untuk posisi bidik & indikator)
        CalculateMouseWorldPosition();

        HandleRotation();

        // 3. FIX DOUBLE MOVE: Gabungkan kecepatan horizontal dan vertikal lalu gerakkan SEKALI SAJA
        Vector3 finalVelocity = (moveDirection * moveSpeed) + verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void CalculateMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            
            // Selalu update posisi target agar sinkron di semua script
            GetMouseTargetPosition = mouseWorldPos;

            // Update posisi & rotasi lingkaran merah di sini (Hemat Raycast!)
            if (aimIndicator != null)
            {
                aimIndicator.position = new Vector3(mouseWorldPos.x, transform.position.y + 0.02f, mouseWorldPos.z);
                aimIndicator.Rotate(Vector3.forward, 50f * Time.deltaTime, Space.Self);
            }
        }
    }

    public void AimTowardsMouse()
    {
        // Menggunakan posisi world dari kalkulasi tunggal di atas
        Vector3 direction = new Vector3(
            GetMouseTargetPosition.x - transform.position.x,
            0f,
            GetMouseTargetPosition.z - transform.position.z
        );

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }

    private void CalculateGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f; // Nilai penahan agar menempel di tanah

        verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void CalculateMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(x, 0f, z).normalized;

        if (animator == null) return;

        Vector3 localMove = transform.InverseTransformDirection(moveDirection);
        float smoothSpeed = Time.deltaTime * 5f;
        animator.SetFloat(AnimHorizontal, Mathf.MoveTowards(animator.GetFloat(AnimHorizontal), localMove.x, smoothSpeed));
        animator.SetFloat(AnimVertical, Mathf.MoveTowards(animator.GetFloat(AnimVertical), localMove.z, smoothSpeed));
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {
            // Klik kanan → kunci hadapan ke arah kursor mouse
            AimTowardsMouse();
        }
        else
        {
            // Tidak klik kanan → hadap sesuai arah jalan WASD
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }

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