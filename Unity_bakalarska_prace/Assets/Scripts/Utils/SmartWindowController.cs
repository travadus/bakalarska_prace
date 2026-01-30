using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SmartWindowController : MonoBehaviour
{
    [Header("Komponenty")]
    public RectTransform windowRect;
    public TMP_InputField inputField;
    public RectTransform contentRect; // Content uvnitø ScrollView

    [Header("Nastavení Minimální velikosti")]
    public float minWidth = 300f;
    // Výpoèet: (FontSize * LineHeight * 4) + Padding. 
    // Pokud máš font 20 a øádkování 1.2, jeden øádek je cca 24px. 4 øádky = cca 100px.
    public float minHeight = 150f;

    [Header("Auto-Resize Settings")]
    public float widthPadding = 50f; // Místo navíc vpravo (pro èísla øádkù a okraj)

    private void Start()
    {
        // Posloucháme zmìny v textu pro automatické rozšiøování
        inputField.onValueChanged.AddListener(OnTextChanged);

        // Místo enableWordWrapping = false použijeme toto:
        inputField.textComponent.textWrappingMode = TextWrappingModes.NoWrap;

        // Overflow mode (aby text "utíkal" doprava a nebyl oøíznutý)
        inputField.textComponent.overflowMode = TextOverflowModes.ScrollRect;
    }

    // --- LOGIKA 1: AUTOMATICKÉ ROZŠIØOVÁNÍ (Když píšeš) ---
    private void OnTextChanged(string text)
    {
        // Zmìøíme, jak široký je text na nejdelším øádku
        float textWidth = inputField.textComponent.preferredWidth;

        // Pokud je text širší než aktuální okno (mínus padding), roztáhneme okno
        float requiredWidth = textWidth + widthPadding;

        // Roztáhneme okno pouze pokud je text delší než aktuální šíøka okna
        // A zároveò respektujeme, že uživatel mohl okno ruènì zvìtšit víc, než je text.
        if (requiredWidth > windowRect.sizeDelta.x)
        {
            SetWindowSize(requiredWidth, windowRect.sizeDelta.y);
        }
    }

    // --- LOGIKA 2: MANUÁLNÍ RESIZE (Voláno z úchytù) ---

    public void OnDragResize(Vector2 deltaDrag, bool horizontal, bool vertical)
    {
        Vector2 newSize = windowRect.sizeDelta;

        if (horizontal) newSize.x += deltaDrag.x;
        if (vertical) newSize.y -= deltaDrag.y; // Y je v UI èasto invertované (taháš dolù = minus y ve screen space, ale plus velikost)

        // Aplikujeme limity

        // 1. Spoèítáme minimální šíøku podle textu (text musí být vždy vidìt)
        float currentTextMinWidth = inputField.textComponent.preferredWidth + widthPadding;
        // Finální minimální šíøka je buï základní (300), nebo ta, co vyžaduje text (pokud je delší)
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