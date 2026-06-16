using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public PlayerHP playerHP;
    public RectTransform fill;

    private float maxWidth;

    void Start()
    {
        maxWidth = fill.sizeDelta.x;
        Debug.Log("maxWidth: " + maxWidth); // cek apakah 0 atau tidak
    }

    void Update()
    {

        // Update tampilan fill bar
        float percent = (float)playerHP.currentHP / playerHP.maxHP;
        fill.sizeDelta = new Vector2(
            maxWidth * percent,
            fill.sizeDelta.y
        );
    }
}