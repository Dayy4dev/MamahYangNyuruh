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

    public void ProcessBoxSelection(string chosenEffect, CandyBox chosenBox)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        PlayerHealth healthComponent = player.GetComponent<PlayerHealth>();
        PlayerAttack attackComponent = player.GetComponent<PlayerAttack>();

        // --- LOGIKA EFEK PERMEN (Tetap sama seperti sistem sebelumnya) ---
        switch (chosenEffect)
        {
            case "Heal":
                if (healthComponent != null) healthComponent.Heal(healthComponent.GetMaxHealth());
                break;
            case "MaxHP":
                if (healthComponent != null) healthComponent.IncreaseMaxHealth(maxHpIncreaseAmount);
                break;
            case "BuffDamage":
                if (attackComponent != null) attackComponent.ApplyPermanentDamageBuff(buffDamageAmount);
                break;
            case "InstantDamage":
                if (healthComponent != null)
                {
                    int currentHp = healthComponent.GetCurrentHealth();
                    int maxHp = healthComponent.GetMaxHealth();
                    if (currentHp < (maxHp * 0.10f)) {
                        Debug.Log("[Partner Effect] Proteksi Aktif.");
                    } else {
                        healthComponent.TakeDamage(Mathf.RoundToInt(currentHp * 0.50f));
                    }
                }
                break;
        }
        // -----------------------------------------------------------------

        // MODIFIKASI: Mengatur loop reveal untuk semua box permen yang ada di room
        foreach (CandyBox box in candyBoxes)
        {
            if (box != null)
            {
                // Jika box ini adalah objek yang diklik player, kirim nilai true (terbang)
                // Jika bukan, kirim nilai false (cuma keliatan isinya lalu hancur)
                bool isThisTheChosenOne = (box == chosenBox);
                box.RevealAndCleanUp(isThisTheChosenOne);
            }
        }

        // Hancurkan objek Partner utamanya juga setelah berinteraksi jika diperlukan
        // Destroy(gameObject, 1.6f); 

        OnCandySelected?.Invoke();
    }
}