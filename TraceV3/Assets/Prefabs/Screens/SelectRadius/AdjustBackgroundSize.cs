using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdjustBackgroundSize : MonoBehaviour
{
    public RectTransform backgroundImage; // Assign the RectTransform of your Image background here
    public Vector2 padding; // Extra padding around the text
    [SerializeField] private TextMeshProUGUI textMeshPro;

    public string Text
    {
        get
        {
            return textMeshPro.text;
        }
        set
        {
            textMeshPro.text = value;
            AdjustSize();
        }
    }
    

    void AdjustSize()
    {
        if (backgroundImage && textMeshPro)
        {
            var textBounds = textMeshPro.GetRenderedValues(false);
            backgroundImage.sizeDelta = new Vector2(textBounds.x + padding.x, textBounds.y + padding.y);
        }
    }
}