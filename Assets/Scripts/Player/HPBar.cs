using UnityEngine;
using UnityEngine.UI;
using TMPro; // Needed to control the TextMeshPro component

public class HPBar : MonoBehaviour
{
    [Header("Dependencies")]
    public HealthData healthData; // Drag your 'PlayerHPAsset' file here
    public Image fillImage;         // Drag your 'Health Bar Fill' image here
    public TextMeshProUGUI hpText;  // Drag your UI 'Text (TMP)' here

    void Update()
    {
        // Debug Test Tool: Press Spacebar to test damage functionality
        if (Input.GetKeyDown(KeyCode.Space) && healthData != null)
        {
            healthData.TakeDamage(10);
            Debug.Log("Spacebar pressed. Player took 10 damage! Current HP: " + healthData.currentHP);
        }

        // Continually push data values to screen UI elements
        if (healthData != null)
        {
            // 1. Update the horizontal fill slider amount
            if (fillImage != null)
            {
                float percent = (float)healthData.currentHP / healthData.maxHP;
                fillImage.fillAmount = percent;
            }

            // 2. Format and display the health numbers text (e.g., "100 / 100")
            if (hpText != null)
            {
                hpText.text = healthData.currentHP + " / " + healthData.maxHP;
            }
        }
    }
}