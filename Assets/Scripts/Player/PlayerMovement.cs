using UnityEngine;

[AddComponentMenu("Player Movement and Camera Controller")]
public class PlayerMovement : MonoBehaviour
{

    [Space]
    [Header("Movement Settings")]

    private CharacterController controller;

    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;

    [Space]
    [Header("Weapon Settings")]
    [Tooltip("Masukkan script HandCannon yang ada di senjata karakter")]
    public HandCannon equippedWeapon;


    void Start()
    {
        controller = GetComponent<CharacterController>();    
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0, z).normalized;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (move != Vector3.zero && !Input.GetMouseButton(1))
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,10f * Time.deltaTime);
        }
        if (Input.GetMouseButton(1))
        {
            PlayerDirection();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (equippedWeapon != null)
                {
                    // Debug.Log("Shoot!");
                    equippedWeapon.Shoot();
                }
            }
        }
    }
    
    public void PlayerDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            
            Vector3 direction = new Vector3(mouseWorldPos.x - transform.position.x,0f,mouseWorldPos.z - transform.position.z);
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }
}