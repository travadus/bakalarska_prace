using UnityEngine;
using TMPro; // Nutné pro Text Mesh Pro
using System; // Nutné pro práci s DateTime

public class PriceDisplayUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI priceText; // Sem pøetáhni ten Text v Canvasu

    [Header("Format")]
    [SerializeField] private string currency = " EUR/MWh";

    private void Start()
    {
        // 1. Bezpeènì zkontrolujeme, jestli TimeSystem existuje
        if (TimeSystem.Instance != null)
        {
            // 2. Pøihlásíme se k odbìru "tikání"
            // Pokaždé, když TimeSystem tikne, spustí se naše metoda UpdatePriceText
            TimeSystem.Instance.OnTick += UpdatePriceText;

            // 3. Provedeme první aktualizaci hned teï (abychom neèekali na první tik)
            UpdatePriceText(TimeSystem.Instance.CurrentDateTime);
        }
        else
        {
            Debug.LogError("Error: Ve scénì chybí TimeSystem!");
        }
    }

    private void OnDestroy()
    {
        // Když se objekt nièí (napø. vypnutí hry), slušnì se odhlásíme z odbìru
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick -= UpdatePriceText;
        }
    }

    // Tato metoda se volá automaticky každých 10 herních minut
    private void UpdatePriceText(DateTime time)
    {
        // Získáme aktuální cenu z GameAPI (které si to bere z MarketManageru)
        float currentPrice = GameAPI.GetCurrentPrice();

        // Nastavíme text
        // "F2" znamená, že chceme èíslo zaokrouhlit na 2 desetinná místa
        priceText.text = $"Price: {currentPrice:F2}{currency}";

        // --- BONUS: Zmìna barvy podle ceny ---
        if (currentPrice < 0)
        {
            priceText.color = Color.green; // Záporná cena = peníze zdarma (Zelená)
        }
        else if (currentPrice > 100)
        {
            priceText.color = Color.red;   // Drahá elektøina (Èervená)
        }
        else
        {
            priceText.color = Color.white; // Normální cena
        }
    }
}