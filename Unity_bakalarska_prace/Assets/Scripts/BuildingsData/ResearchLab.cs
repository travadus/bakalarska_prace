using UnityEngine;

public class ResearchLab : BuildingBase
{
    [Header("Research Settings")]
    public float moneyCostPerTick = 50f;
    public float rpGainPerTick = 10f;

    public bool isActive = true;

    private void Start()
    {
        // 1. Register to TimeSystem for game ticks
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += HandleTick;
        }

        // 2. Register to BuildingsManager using YOUR GENERIC METHOD
        if (BuildingsManager.Instance != null)
        {
            // We pass 'this' and the specific dictionary for labs
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allLabs);
        }
    }

    private void OnDestroy()
    {
        // 1. Unregister from TimeSystem
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick -= HandleTick;
        }

        // 2. Unregister from BuildingsManager using YOUR GENERIC METHOD
        if (BuildingsManager.Instance != null)
        {
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allLabs);
        }
    }

    private void HandleTick(System.DateTime time)
    {
        if (!isActive) return;

        if (EconomyManager.Instance != null && ResearchManager.Instance != null)
        {
            // Try to spend money. "id" is assigned by BuildingsManager.RegisterBuilding
            bool paid = EconomyManager.Instance.TrySpendMoney(moneyCostPerTick, $"Research Lab {id}");

            if (paid)
            {
                ResearchManager.Instance.AddRP(rpGainPerTick);
            }
        }
    }

    // --- Overrides ---

    public override string GetBuildingType()
    {
        return "Research Lab";
    }

    public override string GetStatusText()
    {
        return isActive ? $"Generating {rpGainPerTick} RP/h" : "Paused";
    }

    // Optional: Override for your generic logger in BuildingsManager
    public override string GetDebugInfo()
    {
        return $"Cost: {moneyCostPerTick}, Gain: {rpGainPerTick}";
    }
}