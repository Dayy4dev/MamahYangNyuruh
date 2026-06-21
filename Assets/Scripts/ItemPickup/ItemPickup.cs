using UnityEngine;

[AddComponentMenu("Items/Item Pickup")]
public abstract class ItemPickup : MonoBehaviour
{
    [SerializeField] private Collider pickupCollider;
    protected bool alreadyPickedUp = false;

    protected virtual void Awake()
    {
        if (pickupCollider == null)
            pickupCollider = GetComponent<Collider>();

        if (pickupCollider != null)
            pickupCollider.isTrigger = true;
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
