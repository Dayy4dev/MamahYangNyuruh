using UnityEngine;

// Klik kanan di Project → Create → Items → Candy Data
[CreateAssetMenu(fileName = "NewCandyData", menuName = "Items/Candy Data")]
public class CandyData : ScriptableObject
{
    [Header("Info")]
    public string candyName = "Candy";
    public Sprite icon;

    [Header("Healing")]
    [Tooltip("Jumlah HP yang dipulihkan saat player mengambil candy ini")]
    public int healAmount = 20;
}