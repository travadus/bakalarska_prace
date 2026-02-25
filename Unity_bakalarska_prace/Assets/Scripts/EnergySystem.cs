using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Core simulation engine handling the physical flow of energy across the grid.
/// Manages power generation, distribution, market interactions, and grid loss calculations.
/// </summary>
public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance;

    // --- GRID STATUS ---
    public float PowerBusLevel { get; private set; }
    public float WastedEnergy { get; private set; }

    // --- API COMMANDS ---
    public float PlannedImport { get; set; }
    public float PlannedExport { get; set; }

    private List<IGridActor> standardActors = new List<IGridActor>();
    private List<IGridActor> batteryActors = new List<IGridActor>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (TimeSystem.Instance != null)
            TimeSystem.Instance.OnTick += SimulateEnergyFlow;
    }

    public void RegisterActor(IGridActor actor)
    {
        if (actor is BatteryBuilding)
        {
            if (!batteryActors.Contains(actor)) batteryActors.Add(actor);
        }
        else
        {
            if (!standardActors.Contains(actor)) standardActors.Add(actor);
        }
    }

    public void UnregisterActor(IGridActor actor)
    {
        if (actor is BatteryBuilding)
        {
            if (batteryActors.Contains(actor)) batteryActors.Remove(actor);
        }
        else
        {
            if (standardActors.Contains(actor)) standardActors.Remove(actor);
        }
    }

    // --- MAIN SIMULATION LOOP ---

    /// <summary>
    /// Executes the primary energy flow simulation.
    /// </summary>
    /// <param name="time">The current date and time.</param>
    private void SimulateEnergyFlow(System.DateTime time)
    {
        CollectPrimarySupplies();
        BalanceGridWithStorage();
        DistributeDemand();
        ProcessExport();
        CalculateWaste();
    }

    // --- CORE FLOW METHODS ---

    /// <summary>
    /// Gathers energy from explicit market imports and all registered producing actors (excluding storage).
    /// </summary>
    private void CollectPrimarySupplies()
    {
        // 1. Process external market imports first
        if (PlannedImport > 0)
        {
            PowerBusLevel = PlannedImport;

            if (PlayerScriptEngine.Instance != null)
            {
                PlayerScriptEngine.Instance.LogMessage($"GRID INPUT: +{PlannedImport} MWh arrived from Import.", Color.green);
            }

            PlannedImport = 0f;
        }
        else
        {
            PowerBusLevel = 0f;
        }

        // 2. Extract available energy from local grid sources (e.g., Solar, Batteries)
        foreach (var actor in standardActors)
        {
            float supply = actor.GetAvailableSupply();
            if (supply > 0)
            {
                actor.ExtractEnergy(supply);
                PowerBusLevel += supply;
            }
        }
    }

    /// <summary>
    /// Evaluates the total grid demand against the current power bus level.
    /// Interacts with battery storage to either discharge missing energy or charge surplus energy.
    /// </summary>
    private void BalanceGridWithStorage()
    {
        // Calculate total required energy (Local consumers + Planned Export)
        float totalDemand = PlannedExport;
        foreach (var actor in standardActors)
        {
            totalDemand += actor.GetRequestedDemand();
        }

        float netBalance = PowerBusLevel - totalDemand;

        if (netBalance < 0)
        {
            // Grid deficit: Request exact missing amount from battery storage
            float missingEnergy = Mathf.Abs(netBalance);
            float extractedFromBatteries = 0f;

            foreach (var battery in batteryActors)
            {
                float available = battery.GetAvailableSupply();
                if (available > 0)
                {
                    float amountToTake = Mathf.Min(available, missingEnergy - extractedFromBatteries);
                    battery.ExtractEnergy(amountToTake);
                    extractedFromBatteries += amountToTake;

                    if (extractedFromBatteries >= missingEnergy) break;
                }
            }

            PowerBusLevel += extractedFromBatteries;
        }
        else if (netBalance > 0)
        {
            // Grid surplus: Store excess energy in available battery storage
            float surplusEnergy = netBalance;

            foreach (var battery in batteryActors)
            {
                float spaceAvailable = battery.GetRequestedDemand();
                if (spaceAvailable > 0)
                {
                    float chargeAmount = Mathf.Min(spaceAvailable, surplusEnergy);
                    battery.ReceiveEnergy(chargeAmount);
                    surplusEnergy -= chargeAmount;

                    if (surplusEnergy <= 0) break; // All surplus stored
                }
            }

            // Adjust the bus level after charging batteries
            PowerBusLevel = totalDemand + surplusEnergy;
        }
    }

    /// <summary>
    /// Fulfills planned market exports if sufficient energy is available on the bus.
    /// Handles revenue generation and dumping penalties for negative market prices.
    /// </summary>
    private void ProcessExport()
    {
        if (PlannedExport > 0)
        {
            float amountToSell = Mathf.Min(PowerBusLevel, PlannedExport);

            if (amountToSell > 0)
            {
                PowerBusLevel -= amountToSell;

                if (EconomyManager.Instance != null)
                {
                    float price = EconomyManager.Instance.GetCurrentElectricityPrice();
                    float totalRevenue = amountToSell * price;

                    if (totalRevenue >= 0)
                    {
                        EconomyManager.Instance.AddMoney(totalRevenue, "Export Revenue");

                        if (PlayerScriptEngine.Instance != null)
                            PlayerScriptEngine.Instance.LogMessage($"SOLD: {amountToSell:F1} MWh for {totalRevenue:F2} €", Color.green);
                    }
                    else
                    {
                        float penalty = Mathf.Abs(totalRevenue);
                        EconomyManager.Instance.SubtractMoney(penalty, "Export Dump Fee");

                        if (PlayerScriptEngine.Instance != null)
                            PlayerScriptEngine.Instance.LogMessage($"DUMPED: {amountToSell:F1} MWh (Cost: {penalty:F2} €)", Color.red);
                    }
                }
            }

            PlannedExport = 0f;
        }
    }

    /// <summary>
    /// Distributes available energy to registered consuming actors.
    /// Initiates a brownout sequence if total demand exceeds available supply.
    /// </summary>
    private void DistributeDemand()
    {
        foreach (var actor in standardActors)
        {
            float demand = actor.GetRequestedDemand();
            if (demand > 0)
            {
                if (PowerBusLevel >= demand)
                {
                    PowerBusLevel -= demand;
                    actor.ReceiveEnergy(demand);
                }
                else
                {
                    // Brownout scenario: The actor receives only the remaining fraction of power, and the grid is depleted.
                    actor.ReceiveEnergy(PowerBusLevel);
                    PowerBusLevel = 0f;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Forces the grid to supply a specific amount of energy immediately.
    /// </summary>
    public bool TryConsumeInstantEnergy(float amount)
    {
        if (PowerBusLevel >= amount)
        {
            PowerBusLevel -= amount;
            return true;
        }

        float missingEnergy = amount - PowerBusLevel;

        float availableFromBatteries = 0f;
        foreach (var battery in batteryActors)
        {
            if (battery is BatteryBuilding bat && bat.currentMode == BatteryMode.Discharging)
            {
                availableFromBatteries += bat.GetAvailableSupply();
            }
        }

        if (PowerBusLevel + availableFromBatteries >= amount)
        {
            PowerBusLevel = 0f;

            float extracted = 0f;
            foreach (var battery in batteryActors)
            {
                if (battery is BatteryBuilding bat && bat.currentMode == BatteryMode.Discharging)
                {
                    float toTake = Mathf.Min(bat.GetAvailableSupply(), missingEnergy - extracted);
                    bat.ExtractEnergy(toTake);
                    extracted += toTake;

                    if (extracted >= missingEnergy) break;
                }
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates energy dissipation for surplus power remaining on the grid.
    /// </summary>
    private void CalculateWaste()
    {
        if (PowerBusLevel > 0)
        {
            float surplus = PowerBusLevel;

            // Applies a rational decay curve to simulate physical grid resistance, 
            // where higher loads yield progressively higher dissipation losses.
            float keptEnergy = surplus / (1.0f + (surplus / 50.0f));

            WastedEnergy = surplus - keptEnergy;
            PowerBusLevel = keptEnergy;

            // Threshold log to prevent console spam for minor dissipation
            if (WastedEnergy > 0.1f && PlayerScriptEngine.Instance != null)
            {
                PlayerScriptEngine.Instance.LogSystemMessage($"GRID OVERLOAD: {WastedEnergy:F1} MWh wasted!");
            }
        }
        else
        {
            WastedEnergy = 0f;
            PowerBusLevel = 0f;
        }
    }

    /// <summary>
    /// Safely deducts a specific amount of energy from the power bus if available.
    /// </summary>
    public void ConsumeEnergyFromBus(float amount)
    {
        if (amount > 0 && PowerBusLevel >= amount)
        {
            PowerBusLevel -= amount;
        }
    }
}