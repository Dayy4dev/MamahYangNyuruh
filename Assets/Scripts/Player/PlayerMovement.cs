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

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private CharacterController controller;
    private Animator animator;
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
    }

    void Update()
    {
        if (controller == null) return;

        HandleGravity();
        HandleMovement();
        HandleRotation();
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
        animator.SetFloat(AnimVertical,   Mathf.MoveTowards(animator.GetFloat(AnimVertical),   localMove.z, smoothSpeed));
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