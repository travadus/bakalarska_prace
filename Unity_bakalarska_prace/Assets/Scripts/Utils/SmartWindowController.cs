using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary> Manages adaptive window resizing (auto-expansion and manual drag). </summary>
public class SmartWindowController : MonoBehaviour
{
    [Header("Components")]
    public RectTransform windowRect;
    public TMP_InputField inputField;
    public RectTransform contentRect;

    [Header("Constraints")]
    public float minWidth = 300f;
    public float minHeight = 150f;
    public float widthPadding = 50f;

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnTextChanged);

        inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;
        inputField.textComponent.overflowMode = TextOverflowModes.ScrollRect;
    }

    /// <summary> Auto-expands window width based on text preferred width. </summary>
    private void OnTextChanged(string text)
    {
        float requiredWidth = inputField.textComponent.preferredWidth + widthPadding;

        if (requiredWidth > windowRect.sizeDelta.x)
        {
            SetWindowSize(requiredWidth, windowRect.sizeDelta.y);
        }
    }

    /// <summary> Manual resize logic triggered by UI handles. </summary>
    public void OnDragResize(Vector2 deltaDrag, bool horizontal, bool vertical)
    {
        Vector2 newSize = windowRect.sizeDelta;

        if (horizontal) newSize.x += deltaDrag.x;
        if (vertical) newSize.y -= deltaDrag.y;

        float currentTextMinWidth = inputField.textComponent.preferredWidth + widthPadding;
        float dynamicMinWidth = Mathf.Max(minWidth, currentTextMinWidth);

        newSize.x = Mathf.Max(newSize.x, dynamicMinWidth);
        newSize.y = Mathf.Max(newSize.y, minHeight);

        SetWindowSize(newSize.x, newSize.y);
    }

    private void SetWindowSize(float x, float y)
    {
        windowRect.sizeDelta = new Vector2(x, y);
    }
}