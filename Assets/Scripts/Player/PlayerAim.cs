using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Camera mainCam;

    private void Start()
    {
        // Menyimpan referensi Main Camera di awal agar performa lebih ringan
        mainCam = Camera.main; 
    }

    private void Update()
    {
        // Mengecek apakah pemain menekan klik kiri (Fire1)
        
        if (Input.GetButtonDown("Fire1")) 
        {
            AimTowardsMouse();
        }
    }

    private void AimTowardsMouse()
    {
        // Membuat garis imajiner (Ray) dari kamera melalui posisi mouse di layar
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // Membuat lantai imajiner (Plane) yang rata persis di ketinggian karakter saat ini
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;

        // Mengecek di mana garis dari mouse tadi menabrak lantai imajiner
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // Mendapatkan kordinat 3D pasti dari titik tabrakan tersebut
            Vector3 targetPoint = ray.GetPoint(rayDistance);

            // Menghitung arah dari karakter ke titik target
            Vector3 lookDirection = targetPoint - transform.position;
            
            // Mengunci sumbu Y agar karakter tidak miring (mendongak/menunduk)
            lookDirection.y = 0; 

            // Memutar karakter menghadap arah tersebut secara instan
            if (lookDirection != Vector3.zero)
            {
                transform.forward = lookDirection;
            }
        }
    }
}