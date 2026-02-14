using UnityEngine;
using TMPro;

public class GameInfoUI : MonoBehaviour
{
    [Header("UI Elementy (Pøetáhni z Canvasu)")]
    public TextMeshProUGUI priceText;   // Vpravo nahoøe: Cena
    public TextMeshProUGUI weatherText; // Vlevo nahoøe: Poèasí

    // --- NOVÉ ---
    public TextMeshProUGUI gridText;    // Vpravo nahoøe (pod cenou): Stav sítì
    // ------------

    private void Update()
    {
        UpdateEconomy();
        UpdateWeather();
        UpdateGrid(); // Voláme novou metodu
    }

    private void UpdateEconomy()
    {
        if (EconomyManager.Instance != null && priceText != null)
        {
            float price = EconomyManager.Instance.GetCurrentElectricityPrice();

            // Barvièky: Zelená = Levno (pod 15), Èervená = Draho, Modrá = Záporná cena (Zisk)
            string colorHex = "#FFFFFF";

            if (price < 0) colorHex = "#00FFFF";      // Cyan (Záporná)
            else if (price < 15) colorHex = "#00FF00"; // Green (Levno)
            else if (price > 100) colorHex = "#FF0000"; // Red (Draho)

            priceText.text = $"El. market price:\n<color={colorHex}>{price:F2} €/MWh</color>";
        }
    }

    private void UpdateWeather()
    {
        if (WeatherSystem.Instance != null && weatherText != null)
        {
            // Získání dat
            float sun = WeatherSystem.Instance.CurrentWeather.SunIntensity * 100f;
            float wind = WeatherSystem.Instance.CurrentWeather.WindIntensity * 100f;
            float clouds = WeatherSystem.Instance.CurrentWeather.CloudDensity * 100f;

            // Formátování
            string sunInfo = $"Sun: {sun:F0}%";
            string windInfo = $"Wind: {wind:F0}%";
            string cloudInfo = $"Clouds: {clouds:F0}%";

            weatherText.text = $"{sunInfo}\n{cloudInfo}\n{windInfo}";
        }
    }

    private void UpdateGrid()
    {
        if (EnergySystem.Instance != null && gridText != null)
        {
            float realEnergy = EnergySystem.Instance.PowerBusLevel;
            float import = EnergySystem.Instance.PlannedImport;
            float export = EnergySystem.Instance.PlannedExport; // Musíme naèíst i export

            // 1. Hlavní hodnota
            // Chtìl jsi, aby Grid Bus byl bílý (#FFFFFF)
            string finalText = $"Grid Bus:\n<color=#FFFFFF><size=100%>{realEnergy:F1} MWh</size></color>";

            // 2. Podøádek (Plánovaná zmìna)
            // Chtìl jsi to šedivé (#AAAAAA) a formát "+X MWh" nebo "-X MWh"

            if (import > 0)
            {
                // Import = Plusová hodnota (pøijde do sítì)
                finalText += $"\n<size=70%><color=#AAAAAA>+{import:F1} MWh</color></size>";
            }
            else if (export > 0)
            {
                // Export = Mínusová hodnota (zmizí ze sítì)
                finalText += $"\n<size=70%><color=#AAAAAA>-{export:F1} MWh</color></size>";
            }

            gridText.text = finalText;
        }
    }
}