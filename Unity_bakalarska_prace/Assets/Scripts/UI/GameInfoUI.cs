using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Game info UI
/// </summary>
public class GameInfoUI : MonoBehaviour
{
    [Header("UI Elementy (Pøetáhni z Canvasu)")]
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI weatherText;
    public TextMeshProUGUI gridText;

    /// <summary>
    /// Regularly updates the HUD.
    /// </summary>
    private void Update()
    {
        UpdateEconomy();
        UpdateWeather();
        UpdateGrid();
    }

    /// <summary>
    /// Retrieves current electricity market prices.
    /// </summary>
    private void UpdateEconomy()
    {
        if (EconomyManager.Instance != null && priceText != null)
        {
            float price = EconomyManager.Instance.GetCurrentElectricityPrice();

            string colorHex = "#FFFFFF";

            if (price < 0) colorHex = "#00FFFF";       // Negative price
            else if (price < 15) colorHex = "#00FF00"; // Low market price
            else if (price > 100) colorHex = "#FF0000"; // High market price

            priceText.text = $"El. market price:\n<color={colorHex}>{price:F2} €/MWh</color>";
        }
    }

    /// <summary>
    /// Polls the weather system.
    /// </summary>
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

    /// <summary>
    /// Displays the current state of the energy bus.
    /// </summary>
    private void UpdateGrid()
    {
        if (EnergySystem.Instance != null && gridText != null)
        {
            float realEnergy = EnergySystem.Instance.PowerBusLevel;
            float import = EnergySystem.Instance.PlannedImport;
            float export = EnergySystem.Instance.PlannedExport;

            // Base Grid Bus status
            string finalText = $"Grid Bus:\n<color=#FFFFFF><size=100%>{realEnergy:F1} MWh</size></color>";

            if (import > 0)
            {
                finalText += $"\n<size=70%><color=#AAAAAA>+{import:F1} MWh</color></size>";
            }
            else if (export > 0)
            {
                finalText += $"\n<size=70%><color=#AAAAAA>-{export:F1} MWh</color></size>";
            }

            gridText.text = finalText;
        }
    }
}