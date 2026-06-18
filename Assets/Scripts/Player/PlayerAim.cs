using UnityEngine;

// NOTE: Script ini nggak dipake langsung sekarang karena logika AimTowardsMouse()
// sudah ada di PlayerMovement.cs. Kalau mau dipisah (misal untuk sistem aim terpisah
// dari movement), assign script ini ke objek aim-nya dan panggil AimTowardsMouse()
// dari PlayerMovement.Update() via GetComponent<PlayerAim>().
public class PlayerAim : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        mainCam = Camera.main;
    }

    public void AimTowardsMouse()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
                transform.forward = lookDirection;
        }
    }
}