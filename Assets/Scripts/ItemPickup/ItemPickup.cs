using UnityEngine;

[AddComponentMenu("Items/Item Pickup")]
public abstract class ItemPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private Collider pickupCollider;
    protected bool alreadyPickedUp = false;

    [Header("Animation Settings")]
    [SerializeField] private bool enableAnimation = true;
    
    [Tooltip("Kecepatan rotasi X, Y, Z")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);
    
    [Tooltip("Tinggi maksimal naik-turun")]
    [SerializeField] private float floatAmplitude = 0.25f;
    
    [Tooltip("Kecepatan naik-turun")]
    [SerializeField] private float floatSpeed = 2f;

    private float startY; // Menyimpan posisi Y awal

    protected virtual void Awake()
    {
        if (pickupCollider == null)
            pickupCollider = GetComponent<Collider>();

        if (pickupCollider != null)
            pickupCollider.isTrigger = true;

        startY = transform.position.y; 
    }

    protected virtual void Update()
    {
        if (enableAnimation && !alreadyPickedUp)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);

            float newY = startY + (Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (alreadyPickedUp) return;

        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            OnPickup(other.gameObject);
            alreadyPickedUp = true;
            Destroy(gameObject);
        }
    }

    protected abstract void OnPickup(GameObject player);
}