using UnityEngine;
using System.Collections.Generic;
using System;

public class PartnerSystem : MonoBehaviour
{
    public event Action OnCandySelected;

    [Header("Candy Settings Data")]
    [SerializeField] private int maxHpIncreaseAmount = 30; // Sesuai request: +30 Max HP
    [SerializeField] private int buffDamageAmount = 10;     // Nilai buff damage tetap diatur dari Inspector

    [Header("Child Box Components")]
    [SerializeField] private CandyBox[] candyBoxes;

    private List<string> candyPool = new List<string> { "Heal", "MaxHP", "BuffDamage", "InstantDamage" };
    private List<string> selectedCandies = new List<string>();

    void Start()
    {
        SetupRandomCandies();
    }

    void SetupRandomCandies()
    {
        List<string> tempPool = new List<string>(candyPool);
        int indexToRemove = UnityEngine.Random.Range(0, tempPool.Count);
        tempPool.RemoveAt(indexToRemove);
        selectedCandies = tempPool;

        for (int i = 0; i < candyBoxes.Length; i++)
        {
            if (candyBoxes[i] != null)
            {
                candyBoxes[i].InitializeBox(selectedCandies[i], this);
            }
        }
    }

    public void ProcessBoxSelection(string chosenEffect)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerHealth healthComponent = player.GetComponent<PlayerHealth>();
        PlayerAttack attackComponent = player.GetComponent<PlayerAttack>();

        switch (chosenEffect)
        {
            case "Heal":
                if (healthComponent != null)
                {
                    // Heal sebesar 100% dari Max HP
                    int maxHp = healthComponent.GetMaxHealth();
                    healthComponent.Heal(maxHp);
                }
                break;

            case "MaxHP":
                if (healthComponent != null)
                {
                    // Tambah max HP sebesar 30 (Heal 10% dari New Max HP diproses di dalam PlayerHealth)
                    healthComponent.IncreaseMaxHealth(maxHpIncreaseAmount);
                }
                break;

            case "BuffDamage":
                if (attackComponent != null)
                {
                    // Berikan buff damage permanen (Infinity)
                    attackComponent.ApplyPermanentDamageBuff(buffDamageAmount);
                }
                break;

            case "InstantDamage":
                if (healthComponent != null)
                {
                    int currentHp = healthComponent.GetCurrentHealth();
                    int maxHp = healthComponent.GetMaxHealth();
                    
                    // Cek batas proteksi: jika HP di bawah 10% dari Max HP, player tidak kena damage
                    float healthThreshold = maxHp * 0.10f;

                    if (currentHp < healthThreshold)
                    {
                        Debug.Log("[Partner Effect] HP Player di bawah 10% dari Max HP! Efek Instant Damage dibatalkan (Proteksi Aktif).");
                    }
                    else
                    {
                        // Berikan damage sebesar 50% dari Current HP saat ini
                        int damageToTake = Mathf.RoundToInt(currentHp * 0.50f);
                        healthComponent.TakeDamage(damageToTake);
                        Debug.Log($"[Partner Effect] Player terkena Instant Damage sebesar 50% dari Current HP (-{damageToTake} HP).");
                    }
                }
                break;
        }

        foreach (CandyBox box in candyBoxes)
        {
            if (box != null) box.RevealAndCleanUp();
        }

        OnCandySelected?.Invoke();
    }
}