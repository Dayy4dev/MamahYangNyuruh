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
    private Weapon[] weapons;           // FIX: tipe Weapon bukan GameObject
    public int startingWeaponIndex = 0; // FIX: default 0, index awal senjata
    public GameObject weaponParent;     // parent yang berisi semua weapon sebagai children

    private int currentWeaponIndex = 0;
    private Weapon equippedWeapon;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        // FIX: isi weapons array dari children weaponParent — cukup sekali di Start, bukan di Update
        if (weaponParent != null)
        {
            weapons = weaponParent.GetComponentsInChildren<Weapon>(true); // true = include inactive
        }

        if (weapons != null && weapons.Length > 0)
        {
            // Pastikan startingWeaponIndex tidak melebihi jumlah weapon
            int safeIndex = Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1);
            EquipWeapon(safeIndex);
        }
        else
        {
            Debug.LogWarning("[PlayerMovement] Weapons array kosong! Pastikan weaponParent sudah di-assign dan punya children dengan component Weapon.");
        }
    }

    private void Update()
    {
        if (controller == null) return;

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

        // rotasi klik kanan prioritas
        if (Input.GetMouseButton(1))
        {
            PlayerDirection();

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (equippedWeapon != null)
                {
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

        if (weapons != null)
        {
            // Ganti senjata dengan tombol angka
            for (int i = 0; i < weapons.Length && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    EquipWeapon(i);
                }
            }

            // Ganti senjata dengan scroll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                int nextIndex = currentWeaponIndex + 1;
                if (nextIndex >= weapons.Length) nextIndex = 0;
                EquipWeapon(nextIndex);
            }
            else if (scroll < 0f)
            {
                int prevIndex = currentWeaponIndex - 1;
                if (prevIndex < 0) prevIndex = weapons.Length - 1;
                EquipWeapon(prevIndex);
            }
        }
    }

    private void EquipWeapon(int index)
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("[PlayerMovement] Weapons array kosong!");
            return;
        }

        if (index < 0 || index >= weapons.Length) return;

        // Matikan semua senjata
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] == null)
            {
                Debug.LogWarning($"[PlayerMovement] Weapon di index {i} null!");
                continue;
            }

            weapons[i].OnWeaponDeactivate();
            weapons[i].gameObject.SetActive(false);

            if (weapons[i].weaponRig != null)
            {
                weapons[i].weaponRig.weight = 0f;
            }
        }

        // Nyalakan senjata yang dipilih
        currentWeaponIndex = index;

        if (weapons[currentWeaponIndex] == null)
        {
            Debug.LogWarning($"[PlayerMovement] Weapon di index {currentWeaponIndex} null, tidak bisa equip!");
            return;
        }

        weapons[currentWeaponIndex].gameObject.SetActive(true);
        equippedWeapon = weapons[currentWeaponIndex];
        equippedWeapon.OnWeaponActivate();

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
        if (other.TryGetComponent<WallFader>(out WallFader wall))
        {
            wall.FadeToTransparent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<WallFader>(out WallFader wall))
        {
            wall.FadeToOpaque();
        }
    }
}