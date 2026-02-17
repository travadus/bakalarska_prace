using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [Header("References")]
    public GameObject tooltipObject;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI contentText;
    public LayoutElement layoutElement;

    [Header("Settings")]
    public int characterWrapLimit = 80; // Kdy se text zalomí

    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        rectTransform = tooltipObject.GetComponent<RectTransform>();
        Hide(); // Na zaèátku schovat
    }

    private void Update()
    {
        if (tooltipObject.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;

            // Offset: Odsazení od špièky kurzoru (20 doprava, 20 dolù)
            float offsetX = 20f;
            float offsetY = 20f;

            // Kontrola, aby okno nevyjelo z obrazovky vpravo
            float rightEdgeLimit = Screen.width - rectTransform.rect.width - offsetX;
            if (mousePos.x > rightEdgeLimit)
            {
                // Pokud jsme moc vpravo, ukážeme tooltip vlevo od myši
                mousePos.x -= (rectTransform.rect.width + offsetX);
            }
            else
            {
                mousePos.x += offsetX;
            }

            // Kontrola, aby okno nevyjelo z obrazovky dole
            float bottomEdgeLimit = rectTransform.rect.height + offsetY;
            if (mousePos.y < bottomEdgeLimit)
            {
                // Pokud jsme moc dole, ukážeme tooltip nad myší
                mousePos.y += offsetY;
            }
            else
            {
                mousePos.y -= offsetY;
            }

            // Nastavení pozice pøímo v pixelech obrazovky
            tooltipObject.transform.position = new Vector3(mousePos.x, mousePos.y, 0f);
        }
    }

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

        // Logika pro zalamování textu (Layout Element)
        int headerLength = headerText.text.Length;
        int contentLength = contentText.text.Length;

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);

        tooltipObject.SetActive(true);

        // Donutit Unity pøekreslit layout hned teï (aby neproblikla špatná velikost)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void Hide()
    {
        tooltipObject.SetActive(false);
    }
}