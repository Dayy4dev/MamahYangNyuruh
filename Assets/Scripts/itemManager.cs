using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void Heal()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Healing item used!");
        Debug.Log("Current Health: " + currentHealth);
    } 


}
