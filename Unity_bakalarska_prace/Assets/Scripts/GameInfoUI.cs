using UnityEngine;
using TMPro;

public class GameInfoUI : MonoBehaviour
{
    [Header("UI Elementy (Pøetáhni z Canvasu)")]
    public TextMeshProUGUI priceText;   // Pro cenu elektøiny
    public TextMeshProUGUI weatherText; // Pro slunce a vítr

    private void Update()
    {
        UpdateEconomy();
        UpdateWeather();
    }

    private void UpdateEconomy()
    {
        if (EconomyManager.Instance != null)
        {
            // Zobrazení ceny elektøiny
            if (priceText != null)
            {
                float price = EconomyManager.Instance.GetCurrentElectricityPrice();

                // Barvièky: Zelená = Levno (pod 50), Èervená = Draho, Modrá = Záporná cena (Zisk)
                string colorHex = "#FFFFFF"; // Bílá default

                if (price < 0) colorHex = "#00FFFF"; // Cyan (Záporná cena)
                else if (price < 15) colorHex = "#00FF00"; // Green (Levno)
                else if (price > 100) colorHex = "#FF0000"; // Red (Draho)

                // PØEKLAD: Cena -> Price
                priceText.text = $"El. market price:\n<color={colorHex}>{price:F2} €/MWh</color>";
            }
        }
    }

    private void UpdateWeather()
    {
        if (WeatherSystem.Instance != null && weatherText != null)
        {
            float sun = WeatherSystem.Instance.CurrentWeather.SunIntensity * 100f;
            float wind = WeatherSystem.Instance.CurrentWeather.WindIntensity * 100f;
            float clouds = WeatherSystem.Instance.CurrentWeather.CloudDensity * 100f;

            string sunInfo = $"Sun: {sun:F0}%";
            string windInfo = $"Wind: {wind:F0}%";
            string cloudInfo = $"Clouds: {clouds:F0}%";

            weatherText.text = $"{sunInfo}\n{cloudInfo}\n{windInfo}";
        }
    }
}