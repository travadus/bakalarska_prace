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

    private List<IGridActor> gridActors = new List<IGridActor>();

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
        if (!gridActors.Contains(actor)) gridActors.Add(actor);
    }

    public void UnregisterActor(IGridActor actor)
    {
        if (gridActors.Contains(actor)) gridActors.Remove(actor);
    }

    // --- MAIN SIMULATION LOOP ---

    /// <summary>
    /// Executes the primary energy flow simulation sequentially: Generation -> Export -> Distribution -> Waste.
    /// </summary>
    /// <param name="time">The current in-game date and time.</param>
    private void SimulateEnergyFlow(System.DateTime time)
    {
        CollectSupplies();
        ProcessExport();
        DistributeDemand();
        CalculateWaste();
    }

    // --- CORE FLOW METHODS ---

    /// <summary>
    /// Gathers energy from explicit market imports and all registered producing actors.
    /// </summary>
    private void CollectSupplies()
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
        foreach (var actor in gridActors)
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
        foreach (var actor in gridActors)
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