using UnityEngine;
using System.Collections.Generic;

public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance;

    public float PowerBusLevel { get; private set; }
    public float WastedEnergy { get; private set; }
    public float PlannedImport { get; set; }

    // Seznam všech úèastníkù sítì (baterie, soláry, továrny...)
    // Už nerozlišujeme typy! Všechno je IGridActor.
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

    // Tuto metodu budou volat budovy pøi postavení (Start)
    public void RegisterActor(IGridActor actor)
    {
        if (!gridActors.Contains(actor)) gridActors.Add(actor);
    }

    // Tuto metodu budou volat budovy pøi znièení (OnDestroy)
    public void UnregisterActor(IGridActor actor)
    {
        if (gridActors.Contains(actor)) gridActors.Remove(actor);
    }

    // HLAVNÍ SMYÈKA - Teï je krásnì èitelná
    private void SimulateEnergyFlow(System.DateTime time)
    {
        // 1. Získání energie (Inputs)
        CollectSupplies();

        // 2. Rozdání energie (Outputs)
        DistributeDemand();

        // 3. Výpoèet ztrát
        CalculateWaste();
    }

    // --- POMOCNÉ METODY ---

    private void CollectSupplies()
    {
        // Zaèneme s importem z burzy
        PowerBusLevel = PlannedImport;
        PlannedImport = 0f;

        // Zeptáme se všech: "Máte nìkdo energii na prodej?"
        foreach (var actor in gridActors)
        {
            float supply = actor.GetAvailableSupply();
            if (supply > 0)
            {
                // Vezmeme energii od aktéra a dáme do sítì
                actor.ExtractEnergy(supply);
                PowerBusLevel += supply;
            }
        }
    }

    private void DistributeDemand()
    {
        // Zeptáme se všech: "Chce nìkdo energii?"
        foreach (var actor in gridActors)
        {
            float demand = actor.GetRequestedDemand();
            if (demand > 0)
            {
                // Máme dost?
                if (PowerBusLevel >= demand)
                {
                    // Ano, uspokojíme poptávku naplno
                    PowerBusLevel -= demand;
                    actor.ReceiveEnergy(demand);
                }
                else
                {
                    // Ne, dáváme jen zbytky (Brownout)
                    actor.ReceiveEnergy(PowerBusLevel);
                    PowerBusLevel = 0f;
                    break; // Sí je prázdná, konèíme
                }
            }
        }
    }

    private void CalculateWaste()
    {
        if (PowerBusLevel > 0)
        {
            float surplus = PowerBusLevel;
            // Naše logaritmická køivka ztrát
            float keptEnergy = surplus / (1.0f + (surplus / 50.0f));

            WastedEnergy = surplus - keptEnergy;
            PowerBusLevel = keptEnergy;

            if (WastedEnergy > 1.0f && PlayerScriptEngine.Instance != null)
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
}