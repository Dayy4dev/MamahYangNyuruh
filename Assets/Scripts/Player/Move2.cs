using UnityEngine;

[AddComponentMenu("Player Movement and Camera Controller")]
public class Move2 : MonoBehaviour
{

    [Space]
    [Header("Movement Settings")]

    private CharacterController controller;
    private Animator animator;

    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;





    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0, z).normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (animator != null)
        {
            //biar relatif arahnya
            Vector3 localMove = transform.InverseTransformDirection(move);

            float currentInputX = animator.GetFloat("Horizontal");
            float currentInputZ = animator.GetFloat("Vertical");

            animator.SetFloat("Horizontal", Mathf.MoveTowards(currentInputX, localMove.x, Time.deltaTime * 5f));
            animator.SetFloat("Vertical", Mathf.MoveTowards(currentInputZ, localMove.z, Time.deltaTime * 5f));
        }
        
        // rotasi klik kanan prioritas mas
        if (Input.GetMouseButton(1))
        {
            PlayerDirection();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Debug.Log("Attack");
            }
        }
        // baru rotasi wasd
        else if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        
    }

    public void PlayerDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);

            Vector3 direction = new Vector3(mouseWorldPos.x - transform.position.x, 0f, mouseWorldPos.z - transform.position.z);

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Jika menabrak trigger milik dinding yang menghalangi
        if (other.TryGetComponent<WallFader>(out WallFader wall))
        {
            wall.FadeToTransparent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Jika keluar dari area belakang dinding
        if (other.TryGetComponent<WallFader>(out WallFader wall))
        {
            wall.FadeToOpaque();
        }
    }
}