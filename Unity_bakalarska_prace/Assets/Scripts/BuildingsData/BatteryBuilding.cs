using UnityEngine;

/// <summary>
/// Represents a battery storage facility.
/// </summary>
public class BatteryBuilding : BuildingBase, IGridActor
{
    [Header("Settings")]
    public float maxStorageCapacity = 100f;
    public float maxInputOutput = 10f;

    [Header("State")]
    public float currentCharge = 0f;
    public BatteryMode currentMode = BatteryMode.Standby;

    private void Awake()
    {
        BuildingName = "Battery";

        currentCharge = 0f;
        currentMode = BatteryMode.Standby;
    }

    public override string GetDebugInfo()
    {
        return $"Capacity: {maxStorageCapacity} kWh, Charge: {currentCharge:F1}";
    }

    private void Start()
    {
        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allBatteries);
        }

        if (EnergySystem.Instance != null)
        {
            EnergySystem.Instance.RegisterActor(this);
        }
    }

    private void OnDestroy()
    {
        if (BuildingsManager.Instance != null)
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allBatteries);

        if (EnergySystem.Instance != null)
            EnergySystem.Instance.UnregisterActor(this);
    }

    public float GetAvailableSupply()
    {
        if (currentMode == BatteryMode.Discharging)
        {
            return Mathf.Min(maxInputOutput, currentCharge);
        }
        return 0f;
    }

    public void ExtractEnergy(float amount)
    {
        currentCharge -= amount;
    }

    public float GetRequestedDemand()
    {
        if (currentMode == BatteryMode.Charging)
        {
            float spaceLeft = maxStorageCapacity - currentCharge;
            return Mathf.Min(maxInputOutput, spaceLeft);
        }
        return 0f;
    }

    public void ReceiveEnergy(float amount)
    {
        currentCharge += amount;
    }

    protected override string GetTooltipHeader()
    {
        return $"Battery Storage #{id}";
    }

    protected override string GetTooltipContent()
    {
        string status = currentMode.ToString();
        string color = "white";

        if (currentMode == BatteryMode.Charging) color = "green";
        if (currentMode == BatteryMode.Discharging) color = "red";

        return $"Energy: {currentCharge:F1} / {maxStorageCapacity} MWh\n" +
               $"Mode: <color={color}>{status}</color>";
    }
}