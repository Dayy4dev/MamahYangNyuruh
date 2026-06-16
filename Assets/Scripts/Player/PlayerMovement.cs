using UnityEngine;

[AddComponentMenu("Player Movement and Camera Controller")]
public class PlayerMovement : MonoBehaviour
{
    [Space]
    [Header("Movement Settings")]
    private CharacterController controller;

    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f; // Kekuatan gravitasi jatuh
    private Vector3 velocity; // Menyimpan kecepatan jatuh player
    private bool isGrounded; // Status apakah menyentuh tanah

    void Start()
    {
        controller = GetComponent<CharacterController>();    
    }

    private void Update()
    {
        // 1. Cek apakah player menyentuh tanah menggunakan fitur Character Controller
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Kunci nilai sedikit di bawah 0 agar deteksi ground stabil
        }

        // 2. Input pergerakan horizontal
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0, z).normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // 3. Hitung gravitasi secara vertikal (Sumbu Y)
        velocity.y += gravity * Time.deltaTime;

        // 4. Jalankan pergerakan jatuh akibat gravitasi
        controller.Move(velocity * Time.deltaTime);
    }

   
}
