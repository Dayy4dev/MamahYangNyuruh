using UnityEngine;

[AddComponentMenu("Player Movement and Camera Controller")]
public class PlayerMovement : MonoBehaviour
{

    [Space]
    [Header("Movement Settings")]

    private CharacterController controller;
    private Animator animator;

    [Tooltip("Movement speed")]
    public float moveSpeed = 2f;

    [Space]
    [Header("Weapon Inventory")]
    [Tooltip("Masukkan semua objek senjata")]
    public Weapon[] weapons; 
    
    private int currentWeaponIndex = 0;
    private Weapon equippedWeapon;


    void Start()
    {
        EquipWeapon(0);
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
                if (equippedWeapon != null)
                {
                    // Debug.Log("Shoot!");
                    equippedWeapon.Attack();
                }
            }
        }
        // baru rotasi wasd
        else if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        for (int i = 0; i < weapons.Length && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipWeapon(i);
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll > 0f)
        {
            int nextIndex = currentWeaponIndex + 1;
            if(nextIndex >= weapons.Length) nextIndex = 0;
            EquipWeapon(nextIndex);
        } else if(scroll < 0f)
        {
            int prevIndex = currentWeaponIndex - 1;
            if(prevIndex < 0) prevIndex = weapons.Length - 1;
            EquipWeapon(prevIndex);
        }
        
    }

    private void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        //mematikaan semua senjata di tangan
        for(int i = 0; i < weapons.Length; i++)
        {
            weapons[i].OnWeaponDeactivate(); // Reset state saat weapon dinonaktifkan
            weapons[i].gameObject.SetActive(false);

            if (weapons[i].weaponRig != null)
            {
                weapons[i].weaponRig.weight = 0f;
            }
        }

        //nyalakan senjata
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].gameObject.SetActive(true);
        equippedWeapon = weapons[currentWeaponIndex];
        equippedWeapon.OnWeaponActivate(); // Resume state saat weapon diaktifkan

        if (equippedWeapon.weaponRig != null)
        {
            equippedWeapon.weaponRig.weight = 1f;
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