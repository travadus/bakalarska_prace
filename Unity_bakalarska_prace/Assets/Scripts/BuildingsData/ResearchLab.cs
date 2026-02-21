using UnityEngine;
using System;

/// <summary>
/// Represents a research facility that converts financial resources into Research Points (RP) over time.
/// </summary>
public class ResearchLab : BuildingBase
{
    [Header("Research Settings")]
    public float moneyCostPerTick = 250f;
    public float rpGainPerTick = 1f;

    private int lastProcessedHour = -1;

    public bool isOperating = true;

    private void Start()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += HandleTick;
        }

        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allResearchLabs);
        }
    }

    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick -= HandleTick;
        }

        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allResearchLabs);
        }
    }

    /// <summary>
    /// Processes the facility's operations upon receiving a global time tick.
    /// Evaluates the transition to a new in-game hour, processes funding deductions, 
    /// grants research points, and automatically halts operations if funds are insufficient.
    /// </summary>
    /// <param name="time">The current game time.</param>
    private void HandleTick(DateTime time)
    {
        if (!isOperating) return;

        if (time.Hour != lastProcessedHour)
        {
            if (EconomyManager.Instance != null && ResearchManager.Instance != null)
            {
                bool paid = EconomyManager.Instance.TrySpendMoney(moneyCostPerTick, $"Research Lab {id} Funding");

                if (paid)
                {
                    lastProcessedHour = time.Hour;
                    ResearchManager.Instance.AddRP(rpGainPerTick);
                }
                else
                {
                    isOperating = false;

                    if (PlayerScriptEngine.Instance != null)
                    {
                        PlayerScriptEngine.Instance.LogMessage($"LAB {id}: Insufficient funds for the next hour. System halted.", Color.red);
                    }
                }
            }
        }
    }

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