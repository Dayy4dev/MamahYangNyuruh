using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Heal()
    {
        currentHealth = Mathf.Clamp(currentHealth + 20, 0, maxHealth);
        Debug.Log("Healing item used!");
        Debug.Log("Current Health: " + currentHealth);
    } 
}
