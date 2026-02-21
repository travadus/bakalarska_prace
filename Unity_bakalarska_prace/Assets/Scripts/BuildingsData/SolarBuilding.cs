using System.Drawing;
using UnityEngine;

/// <summary>
/// Represents a solar power generation facility. 
/// </summary>
public class SolarBuilding : BuildingBase, IGridActor
{
    [Header("Parameters")]
    public float maxOutput = 5.0f;
    public float cleaningCost = 50f;

    [Header("Speed of dirt accumulation")]
    public float dustRate = 0.0002f;

    [Header("State")]
    [Range(0, 1)]
    public float dirtLevel = 0f;

    /// <summary>
    /// Gets the real-time energy production calculated during the last grid cycle.
    /// </summary>
    public float CurrentProduction { get; private set; }

    private void Awake()
    {
        BuildingName = "Solar Farm";
    }

    private void Start()
    {
        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allSolars);
        }

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

    // --- IGRIDACTOR IMPLEMENTATION ---

    /// <summary>
    /// Calculates and retrieves the available energy supply. 
    /// </summary>
    /// <returns>The calculated energy output in MWh.</returns>
    public float GetAvailableSupply()
    {
        float sunIntensity = 0f;
        if (WeatherSystem.Instance != null)
        {
            sunIntensity = WeatherSystem.Instance.CurrentWeather.SunIntensity;
        }

        float efficiency = 1.0f - dirtLevel;
        if (efficiency < 0) efficiency = 0;

        CurrentProduction = maxOutput * sunIntensity * efficiency;

        float change = dustRate * Random.Range(0.8f, 1.2f);
        dirtLevel += change;

        if (dirtLevel > 1.0f) dirtLevel = 1.0f;

        return CurrentProduction;
    }

    public void ExtractEnergy(float amount) { }

    public float GetRequestedDemand() => 0f;

    public void ReceiveEnergy(float amount) { }

    /// <summary>
    /// Restores the solar panels to peak efficiency by removing all accumulated dirt.
    /// </summary>
    public void CleanPanels()
    {
        dirtLevel = 0f;
    }

    public override string GetDebugInfo()
    {
        return $"Max Output: {maxOutput} MWh, Dirt: {dirtLevel * 100:F1}%";
    }

    protected override string GetTooltipHeader()
    {
        return $"Solar Power Plant #{id}";
    }

    protected override string GetTooltipContent()
    {
        string dirtColor = dirtLevel > 0.2f ? "red" : "white";

        return $"Output: {CurrentProduction:F1} / {maxOutput} MWh\n" +
               $"Dirt Level: <color={dirtColor}>{dirtLevel * 100:F1}%</color>\n" +
               $"Clean Cost: {cleaningCost} €";
    }
}