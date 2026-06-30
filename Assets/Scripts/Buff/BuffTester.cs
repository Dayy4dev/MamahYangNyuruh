using UnityEngine;

public class BuffTester : MonoBehaviour
{
    [SerializeField] private PlayerBuffManager buffManager;

    void Update()
    {
        // Tekan tombol H di keyboard untuk menambah Max HP & Heal 30%
        if (Input.GetKeyDown(KeyCode.H))
        {
            buffManager.ApplyHpAndHealBuff();
        }

        // Tekan tombol J di keyboard untuk menambah Damage +50
        if (Input.GetKeyDown(KeyCode.J))
        {
            buffManager.ApplyDamageBuff();
        }
    }
}