using UnityEngine;
using System.Collections.Generic;
using System;

public class PartnerSystem : MonoBehaviour
{
    public event Action OnCandySelected;

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

    public void ProcessBoxSelection(string chosenEffect, CandyBox chosenBox)
    {
        if (LootboxManager.Instance != null)
        {
            LootboxManager.Instance.SetCandyEffectStatus(chosenEffect);
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerHealth healthComponent = player.GetComponent<PlayerHealth>();
        PlayerBuffManager buffManager = player.GetComponent<PlayerBuffManager>();

        // --- LOGIKA EFEK PERMEN (SINKRON DENGAN PLAYERBUFFMANAGER) ---
        switch (chosenEffect)
        {
            case "Heal":
                // Tetap Full Heal biasa (g usah diganti ke buff manager)
                if (healthComponent != null) healthComponent.Heal(healthComponent.GetMaxHealth());
                break;

            case "MaxHP":
                // Permen MaxHP memanggil fungsi Max HP +100 & Heal 30% gabungan
                if (buffManager != null)
                {
                    buffManager.ApplyHpAndHealBuff();
                }
                break;

            case "BuffDamage":
                // Permen BuffDamage memanggil fungsi Damage +50
                if (buffManager != null)
                {
                    buffManager.ApplyDamageBuff();
                }
                break;

            case "InstantDamage":
                if (healthComponent != null)
                {
                    int currentHp = healthComponent.GetCurrentHealth();
                    int maxHp = healthComponent.GetMaxHealth();
                    if (currentHp < (maxHp * 0.10f))
                    {
                        Debug.Log("[Partner Effect] Proteksi Aktif.");
                    }
                    else
                    {
                        healthComponent.TakeDamage(Mathf.RoundToInt(currentHp * 0.50f));
                    }
                }
                break;
        }

        if (LootboxManager.Instance != null)
        {
            LootboxManager.Instance.SetCandyEffectStatus(chosenEffect);
        }

        foreach (CandyBox box in candyBoxes)
        {
            if (box != null)
            {
                bool isThisTheChosenOne = (box == chosenBox);
                box.RevealAndCleanUp(isThisTheChosenOne);
            }
        }

        OnCandySelected?.Invoke();
    }
}