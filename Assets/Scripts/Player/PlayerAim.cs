using UnityEngine;

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
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance);
            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0; 

            if (lookDirection != Vector3.zero)
            {
                transform.forward = lookDirection;
            }
        }
    }
}