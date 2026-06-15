using UnityEngine;

[AddComponentMenu("Player Movement and Camera Controller")]
public class PlayerMovement : MonoBehaviour
{

    [Space]
    [Header("Movement Settings")]

    private CharacterController controller;

    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;





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

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRotation,10f * Time.deltaTime);
        }
    }

}