using UnityEngine;
using System.Collections.Generic;

public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance;

    // --- STAV SÍTÌ ---
    public float PowerBusLevel { get; private set; }
    public float WastedEnergy { get; private set; }

    // PØÍKAZY Z GAME API
    public float PlannedImport { get; set; } // Hráè chce koupit (BuyEnergy)
    public float PlannedExport { get; set; } // Hráè chce prodat (SellEnergy) - NOVÉ

    // Seznam úèastníkù
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

    // --- HLAVNÍ SMYÈKA ---
    private void SimulateEnergyFlow(System.DateTime time)
    {
        // 1. PØÍTOK (Import + Baterie Out + Soláry)
        CollectSupplies();

        // 2. EXPORT (Prodej na burzu) - TOTO JE NUTNÉ PRO FIX
        ProcessExport();

        // 3. ODBÌR (Baterie In + Mìsto)
        DistributeDemand();

        // 4. ZTRÁTY (Co zbylo)
        CalculateWaste();
    }

    // --- POMOCNÉ METODY ---

    private void CollectSupplies()
    {
        // A) Import z burzy - TADY HLÁSÍME PØÍCHOD
        if (PlannedImport > 0)
        {
            PowerBusLevel = PlannedImport;

            if (PlayerScriptEngine.Instance != null)
            {
                // Tady se energie fyzicky objevila v síti -> ZELENÁ HLÁŠKA
                PlayerScriptEngine.Instance.LogMessage($"GRID INPUT: +{PlannedImport} MWh arrived from Import.", Color.green);
            }

            PlannedImport = 0f;
        }
        else
        {
            // Pokud nebyl import, zaèínáme na nule (nebo pøièítáme k zùstatku, záleží na logice, obvykle reset)
            PowerBusLevel = 0f;
        }

        // B) Zdroje ve høe (Baterie, Soláry...)
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

                        // TADY HLÁSÍME PRODEJ (To je to, co jsi chtìl vidìt)
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
            // Volitelnì: Hláška, že prodej selhal (nemìli jsme energii)
            else if (PowerBusLevel <= 0.01f && PlayerScriptEngine.Instance != null)
            {
                // PlayerScriptEngine.Instance.LogMessage("EXPORT FAILED: Grid is empty.", Color.gray);
            }

            PlannedExport = 0f;
        }
    }

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
                    // Brownout (nedostatek energie)
                    actor.ReceiveEnergy(PowerBusLevel);
                    PowerBusLevel = 0f;
                    break;
                }
            }
        }
    }

    private void CalculateWaste()
    {
        if (PowerBusLevel > 0)
        {
            float surplus = PowerBusLevel;
            // Logaritmická køivka ztrát
            float keptEnergy = surplus / (1.0f + (surplus / 50.0f));

            WastedEnergy = surplus - keptEnergy;
            PowerBusLevel = keptEnergy;

            // Logování jen pøi vìtších ztrátách (nad 0.1 MWh)
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

    public void ConsumeEnergyFromBus(float amount)
    {
        if (amount > 0 && PowerBusLevel >= amount)
        {
            PowerBusLevel -= amount;
        }
    }
}