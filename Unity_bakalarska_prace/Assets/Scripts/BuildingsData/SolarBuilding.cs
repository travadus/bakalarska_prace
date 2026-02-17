using System.Drawing;
using UnityEngine;

public class SolarBuilding : BuildingBase, IGridActor
{
    [Header("Parametry (Edituj v Prefabu)")]
    public float maxOutput = 5.0f; // Maximální výroba (MWh) pøi 100% slunci a èistotì
    public float cleaningCost = 50f; // Cena za vyèištìní

    [Header("Rychlost špinìní")]
    // Kolik špíny pøibude za jednu hodinu (0.0002 = 0.02%)
    public float dustRate = 0.0002f;

    [Header("Stav (Mìní se ve høe)")]
    [Range(0, 1)]
    public float dirtLevel = 0f; // 0 = èistý, 1 = totálnì špinavý (0% výroba)

    // Veøejná vlastnost, aby si ji mohlo pøeèíst GameAPI
    public float CurrentProduction { get; private set; }

    private void Awake()
    {
        // Nastavíme jméno pro tooltip/systém
        BuildingName = "Solar Farm";
    }

    private void Start()
    {
        // 1. Registrace do Evidence (BuildingsManager)
        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allSolars);
        }

        // 2. Registrace do Sítì (EnergySystem)
        if (EnergySystem.Instance != null)
        {
            EnergySystem.Instance.RegisterActor(this);
        }
    }

    private void OnDestroy()
    {
        if (BuildingsManager.Instance != null)
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allSolars);

        if (EnergySystem.Instance != null)
            EnergySystem.Instance.UnregisterActor(this);
    }

    // --- IGridActor: LOGIKA VÝROBY ---

    public float GetAvailableSupply()
    {
        // 1. Zjistíme slunce
        float sunIntensity = 0f;
        if (WeatherSystem.Instance != null)
        {
            sunIntensity = WeatherSystem.Instance.CurrentWeather.SunIntensity;
        }

        // 2. Aplikujeme špínu
        float efficiency = 1.0f - dirtLevel;
        if (efficiency < 0) efficiency = 0;

        // 3. Výpoèet výroby
        CurrentProduction = maxOutput * sunIntensity * efficiency;

        // 4. Simulace špinìní (vždy, i v noci)
        float change = dustRate * Random.Range(0.8f, 1.2f);
        dirtLevel += change;

        // Zarážka, aby to nešlo pøes 100%
        if (dirtLevel > 1.0f) dirtLevel = 1.0f;

        return CurrentProduction;
    }

    // Solár energii jen dává, nikdy ji nebere
    public void ExtractEnergy(float amount) { }

    public float GetRequestedDemand() => 0f;
    public void ReceiveEnergy(float amount) { }

    // --- ÚDRŽBA ---

    public void CleanPanels()
    {
        dirtLevel = 0f;
    }

    public override string GetDebugInfo()
    {
        return $"Max Output: {maxOutput} MWh, Dirt: {dirtLevel * 100:F1}%";
    }

    // --- TOOLTIP IMPLEMENTACE (Pro tvé nové okno) ---

    protected override string GetTooltipHeader()
    {
        return $"Solar Power Plant #{id}";
    }

    protected override string GetTooltipContent()
    {
        // Zobrazíme výrobu a stav zneèištìní
        // Pokud je špína vysoká, text zèervená
        string dirtColor = dirtLevel > 0.2f ? "red" : "white";

        return $"Output: {CurrentProduction:F1} / {maxOutput} MWh\n" +
               $"Dirt Level: <color={dirtColor}>{dirtLevel * 100:F1}%</color>\n" +
               $"Clean Cost: {cleaningCost} €";
    }
}