using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private EquipmentUI equipmentUI;

    [Header("Status Display")]
    [SerializeField] private TextMeshProUGUI atkBuffText;
    [SerializeField] private TextMeshProUGUI maxHpBuffText; 

    [Header("Player Reference")]
    [SerializeField] private GameObject playerObject; 

    [Header("Buff Manager Reference")]
    [Tooltip("Akan dicari secara otomatis jika kosong")]
    [SerializeField] private PlayerBuffManager buffManager;

    private void Start()
    {
        // Cari buff manager di awal jika masih kosong
        EnsureBuffManagerExists();

        if (GameManager.Instance != null)
        {
            EvaluateVisibility(GameManager.Instance.CurrentState);
        }
        else
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            UpdateBuffStatusText();
        }
    }

    private void OnEnable()
    {
        GameManager.OnStateChanged += HandleStateChanged;
        EnsureBuffManagerExists(); // Pastikan kita subscribe ke player yang ada di scene
    }

    private void OnDisable()
    {
        GameManager.OnStateChanged -= HandleStateChanged;

        if (buffManager != null)
        {
            buffManager.OnBuffChanged -= HandleBuffChanged;
        }
    }

    // --- TAMBAHAN FUNGSI PENCARIAN DINAMIS ---
    private void EnsureBuffManagerExists()
    {
        if (buffManager == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                buffManager = player.GetComponent<PlayerBuffManager>();
                
                // Daftarkan event begitu kita menemukan Player di scene
                if (buffManager != null)
                {
                    buffManager.OnBuffChanged -= HandleBuffChanged; // Cegah double subscribe
                    buffManager.OnBuffChanged += HandleBuffChanged;
                }
            }
        }
    }

    private void HandleBuffChanged()
    {
        UpdateBuffStatusText();
    }

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        EvaluateVisibility(newState);
    }

    private void EvaluateVisibility(GameState state) 
    {
        if (inventoryPanel == null) return; 
        bool isWindowOpen = state == GameState.Inventory; 
        inventoryPanel.transform.localScale = isWindowOpen ? Vector3.one : Vector3.zero; 
        
        if (isWindowOpen) { 
            if (equipmentUI != null) equipmentUI.RefreshUI(); 
            UpdateBuffStatusText();
        }
    }

    private void UpdateBuffStatusText()
    {
        // Panggil pencarian lagi buat berjaga-jaga jika player baru saja respawn
        EnsureBuffManagerExists();

        if (buffManager != null)
        {
            int currentAtkBonus = buffManager.GetDamageBuffStack() * 50;
            int currentHpBonus = buffManager.GetHpBuffStack() * 100;

            if (atkBuffText != null)
            {
                atkBuffText.text = currentAtkBonus > 0 
                    ? $"BUFF ATK: <color=green>+{currentAtkBonus}</color> (x{buffManager.GetDamageBuffStack()})" 
                    : "BUFF ATK: ";
            }

            if (maxHpBuffText != null)
            {
                maxHpBuffText.text = currentHpBonus > 0 
                    ? $"BUFF MAX HP: <color=green>+{currentHpBonus}</color> (x{buffManager.GetHpBuffStack()})" 
                    : "BUFF MAX HP: ";
            }
        }
    }
}