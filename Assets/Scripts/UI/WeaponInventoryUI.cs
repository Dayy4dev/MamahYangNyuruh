using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInventoryUI : MonoBehaviour
{
    [Header("Slot 1")]
    [SerializeField] private Image slot1Image;
    [SerializeField] private TMP_Text slot1Name;

    [Header("Slot 2")]
    [SerializeField] private Image slot2Image;
    [SerializeField] private TMP_Text slot2Name;

    public void SetSlot1(Sprite icon, string weaponName)
    {
        slot1Image.sprite = icon;
        slot1Name.text = weaponName;
    }

    public void SetSlot2(Sprite icon, string weaponName)
    {
        slot2Image.sprite = icon;
        slot2Name.text = weaponName;
    }
}