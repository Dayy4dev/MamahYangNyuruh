using UnityEngine;

public class CombatRoom : MonoBehaviour
{
    public GameObject[] enemies;
    public GameObject doorBlocker;

    public void StartCombat()
    {
        doorBlocker.SetActive(true);

        foreach(GameObject enemy in enemies)
        {
            enemy.SetActive(true);
        }
    }

    public void EndCombat()
    {
        doorBlocker.SetActive(false);
    }
}