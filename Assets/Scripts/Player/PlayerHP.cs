using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP=100;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }
}