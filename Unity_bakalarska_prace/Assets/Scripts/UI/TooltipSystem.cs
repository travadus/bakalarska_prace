using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// A global singleton manager responsible for rendering dynamic tooltips.
/// </summary>
public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [Header("References")]
    public GameObject tooltipObject;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI contentText;
    public LayoutElement layoutElement;

    [Header("Settings")]
    public int characterWrapLimit = 80;

    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        rectTransform = tooltipObject.GetComponent<RectTransform>();
        Hide();
    }

    /// <summary>
    /// Synchronizes the tooltip position with the mouse cursor.
    /// </summary>
    private void Update()
    {
        if (tooltipObject.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;

            float offsetX = 20f;
            float offsetY = 20f;

            float rightEdgeLimit = Screen.width - rectTransform.rect.width - offsetX;
            if (mousePos.x > rightEdgeLimit)
            {
                mousePos.x -= (rectTransform.rect.width + offsetX);
            }
            else
            {
                mousePos.x += offsetX;
            }

            float bottomEdgeLimit = rectTransform.rect.height + offsetY;
            if (mousePos.y < bottomEdgeLimit)
            {
                mousePos.y += offsetY;
            }
            else
            {
                mousePos.y -= offsetY;
            }

            tooltipObject.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }
    }

    /// <summary>
    /// Activates the tooltip with content.
    /// </summary>
    /// <param name="content">The primary body of text.</param>
    /// <param name="header">Optional header.</param>
    public void Show(string content, string header = "")
    {
        if (string.IsNullOrEmpty(header))
        {
            headerText.gameObject.SetActive(false);
        }
        else
        {
            headerText.gameObject.SetActive(true);
            headerText.text = header;
        }

        contentText.text = content;

        int headerLength = headerText.text.Length;
        int contentLength = contentText.text.Length;

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);

        tooltipObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    /// <summary>
    /// Disables the tooltip object.
    /// </summary>
    public void Hide()
    {
        tooltipObject.SetActive(false);
    }
}