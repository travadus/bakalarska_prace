using UnityEngine;
using System;

public class ResearchLab : BuildingBase
{
    [Header("Research Settings")]
    public float moneyCostPerTick = 250f;
    public float rpGainPerTick = 1f;

    private int lastProcessedHour = -1;

    // Renamed from 'isActive' to 'isOperating' to match GameAPI requirements
    public bool isOperating = true;

    private void Start()
    {
        // 1. Register to TimeSystem for game ticks
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += HandleTick;
        }

        // 2. Register to BuildingsManager
        // IMPORTANT: Ensure 'allResearchLabs' exists in BuildingsManager.cs as a public Dictionary
        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allResearchLabs);
        }
    }

    private void OnDestroy()
    {
        // 1. Unregister from TimeSystem
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick -= HandleTick;
        }

        // 2. Unregister from BuildingsManager
        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allResearchLabs);
        }
    }

    /// <summary>
    /// Called every in-game hour.
    /// Consumes money and generates Research Points.
    /// </summary>
    private void HandleTick(DateTime time)
    {
        // 1. Pokud je lab vypnutá, nedìlej nic
        if (!isOperating) return;

        // 2. KONTROLA: Probìhla už v této herní hodinì platba?
        // Tato podmínka se splní jen tehdy, když se v herním èase zmìní hodina (napø. z 8:50 na 9:00)
        if (time.Hour != lastProcessedHour)
        {
            if (EconomyManager.Instance != null && ResearchManager.Instance != null)
            {
                // Zaplatíme za provoz na celou hodinu dopøedu
                bool paid = EconomyManager.Instance.TrySpendMoney(moneyCostPerTick, $"Research Lab {id} Funding");

                if (paid)
                {
                    // Uložíme si aktuální hodinu, aby se v dalším tiku (za 10 min) akce neopakovala
                    lastProcessedHour = time.Hour;

                    // Pøidáme RP za hodinu provozu
                    ResearchManager.Instance.AddRP(rpGainPerTick);
                }
                else
                {
                    // Pokud nemáme peníze, vypneme lab
                    isOperating = false;

                    if (PlayerScriptEngine.Instance != null)
                    {
                        PlayerScriptEngine.Instance.LogMessage($"LAB {id}: Insufficient funds for the next hour. System halted.", Color.red);
                    }
                }
            }
        }
    }

    // --- BuildingBase Overrides ---

    public override string GetBuildingType()
    {
        return "Research Lab";
    }

    public override string GetStatusText()
    {
        if (!isOperating) return "OFFLINE";
        return $"Running (-{moneyCostPerTick} €/h)";
    }

    public override string GetDebugInfo()
    {
        return $"State: {(isOperating ? "ON" : "OFF")} | RP Output: {rpGainPerTick}";
    }

    protected override string GetTooltipHeader()
    {
        return $"Research Lab #{id}";  
    }

    protected override string GetTooltipContent()
    {
        string state = isOperating ? "<color=green>ONLINE</color>" : "<color=red>OFFLINE</color>";
        return $"Status: {state}\n" +
               $"Cost: -{moneyCostPerTick} €/h\n" +
               $"Output: +{rpGainPerTick} RP/h";
    }
}