using UnityEngine;

[AddComponentMenu("Items/Candy Item")]
public class CandyItem : ItemPickup
{
    [Header("Healing")]
    [SerializeField] private int healAmount = 20;

    protected override void OnPickup(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            Debug.Log($"[CandyItem] Healed player for {healAmount} HP!");
        }
        else
        {
            Debug.LogWarning("[CandyItem] Player doesn't have PlayerHealth component!");
        }
    }
}
