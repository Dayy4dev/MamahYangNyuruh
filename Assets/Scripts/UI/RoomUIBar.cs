using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RoomUIBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject uiContainer; // Objek utama UI (Canvas/Panel) agar bisa di-hide/show
    [SerializeField] private Slider barSlider;         // Slider untuk Bossbar merah
    [SerializeField] private TextMeshProUGUI roomText; // Teks untuk nama room

    void Awake()
    {
        // Sembunyikan bar di awal game
        ShowBar(false);
    }

    // Fungsi untuk memunculkan atau menyembunyikan UI Bar
    public void ShowBar(bool state)
    {
        if (uiContainer != null)
            uiContainer.SetActive(state);
    }

    // Fungsi untuk memperbarui teks room saat ini
    public void UpdateRoomText(string roomType)
    {
        if (roomText != null)
        {
            roomText.text = $"Room right now: {roomType}";
        }
    }

    // Fungsi untuk memperbarui nilai bar (berkurang seiring musuh mati)
    public void UpdateBarValue(int currentEnemies, int maxEnemies)
    {
        if (barSlider != null && maxEnemies > 0)
        {
            // Menghitung persentase dari penuh ke kosong
            float progress = (float)currentEnemies / maxEnemies;
            barSlider.value = progress;
        }
    }
}
