using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Core production system managing generation, tracking, and evaluation of all energy contracts.
/// Dynamically scales quotas and rewards based on the player's current infrastructure.
/// </summary>
public class ContractsManager : MonoBehaviour
{
    public static ContractsManager Instance { get; private set; }

    public List<Contract> allContracts = new List<Contract>();
    private int contractCounter = 1;

    // Time tracking variables
    private int lastProcessedHour = -1;
    private int lastProcessedDay = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += ProcessContractsTick;

            lastProcessedHour = TimeSystem.Instance.CurrentDateTime.Hour;
            lastProcessedDay = TimeSystem.Instance.CurrentDateTime.DayOfYear;
        }

        RefreshMarket();
    }

    /// <summary>
    /// Clears unaccepted contracts and generates new ones to fill the market.
    /// </summary>
    private void RefreshMarket()
    {
        // Remove old available contracts and finished ones
        allContracts.RemoveAll(c => c.status == ContractStatus.Available || c.status == ContractStatus.Completed || c.status == ContractStatus.Failed);

        // Always provide 3 fresh contracts
        GenerateScaledContract();
        GenerateScaledContract();
        GenerateScaledContract();

        if (PlayerScriptEngine.Instance != null && lastProcessedDay != -1)
        {
            PlayerScriptEngine.Instance.LogMessage("MARKET UPDATE: New dynamic contracts are available on the market.", Color.cyan);
        }
    }

    /// <summary>
    /// Calculates theoretical player capacity to scale contract difficulty dynamically.
    /// </summary>
    private float CalculatePlayerDailyCapacity()
    {
        // Using GameAPI safely to avoid missing references
        int solarCount = GameAPI.GetSolarCount();
        int batteryCount = GameAPI.GetBatteryCount();

        // Baseline capacity for absolute beginners (e.g., 0 buildings)
        float baselineCapacity = 20f;

        // Approximated values: Assume 1 solar generates ~10 MWh/day, 1 battery provides ~5 MWh flexibility
        float estimatedSolarCapacity = solarCount * 10f;
        float estimatedBatteryCapacity = batteryCount * 5f;

        return Mathf.Max(baselineCapacity, estimatedSolarCapacity + estimatedBatteryCapacity);
    }

    /// <summary>
    /// Generates a contract mathematically balanced around the player's current progression.
    /// </summary>
    private void GenerateScaledContract()
    {
        List<string> keys = ContractDatabase.Configs.Keys.ToList();
        string randomKey = keys[Random.Range(0, keys.Count)];
        ContractConfig config = ContractDatabase.GetConfig(randomKey);

        // 1. Determine base capacity (60% to 150% of what player can currently handle)
        float playerDailyCapacity = CalculatePlayerDailyCapacity();
        float targetDailyMWh = playerDailyCapacity * Random.Range(0.6f, 1.5f);

        // 2. Roll for Tier (Loot Table Logic)
        string selectedTier = "Standard";
        float rewardMultiplier = 1.0f;
        int durationDays = Random.Range(7, 31);

        int roll = Random.Range(0, 100);
        if (roll < 10)
        {
            selectedTier = "Urgent";
            rewardMultiplier = 3.0f;
            durationDays = Random.Range(2, 5);
        }
        else if (roll < 30)
        {
            selectedTier = "VIP";
            rewardMultiplier = 1.5f;
        }

        // 3. Create the contract
        Contract newContract = new Contract
        {
            id = contractCounter++,
            contractType = config.TypeName,
            tier = selectedTier,
            status = ContractStatus.Available,
            satisfaction = 100f,
            durationDays = durationDays,
            cycleType = config.Cycle
        };

        // 4. Mathematical Quota & Penalty Calculation
        if (config.Cycle == ContractCycle.Daily)
        {
            newContract.targetMWhPerCycle = Mathf.Round(targetDailyMWh);
        }
        else // Hourly
        {
            // Spread the daily target across 24 hours
            newContract.targetMWhPerCycle = Mathf.Round((targetDailyMWh / 24f) * 10f) / 10f;
            if (newContract.targetMWhPerCycle < 0.5f) newContract.targetMWhPerCycle = 0.5f; // Minimum safeguard
        }

        // 5. Economy Scaling
        newContract.rewardPerMWh = Mathf.RoundToInt(Random.Range(15f, 35f) * rewardMultiplier);

        // Calculate theoretical total revenue to balance bonuses and penalties
        float cyclesPerDay = config.Cycle == ContractCycle.Hourly ? 24f : 1f;
        float expectedTotalRevenue = newContract.targetMWhPerCycle * cyclesPerDay * newContract.rewardPerMWh * durationDays;

        // Bonus is ~10-20% of total contract value. Penalty is ~30-50% of total value.
        newContract.completionBonus = Mathf.RoundToInt((expectedTotalRevenue * Random.Range(0.1f, 0.2f)) / 100f) * 100f;
        newContract.failPenalty = Mathf.RoundToInt((expectedTotalRevenue * Random.Range(0.3f, 0.5f)) / 100f) * 100f;

        // Missed step penalty is roughly 2x the value of the energy they failed to deliver
        newContract.missedStepPenalty = Mathf.RoundToInt(newContract.targetMWhPerCycle * newContract.rewardPerMWh * 2f);

        newContract.daysRemaining = newContract.durationDays;
        allContracts.Add(newContract);
    }

    public void AcceptContract(int id)
    {
        Contract c = allContracts.Find(x => x.id == id);
        if (c != null && c.status == ContractStatus.Available)
        {
            c.status = ContractStatus.Active;
            if (PlayerScriptEngine.Instance != null)
            {
                PlayerScriptEngine.Instance.LogMessage($"ACCEPTED {c.tier} CONTRACT: {c.contractType} (ID: {c.id}). Duration: {c.durationDays} days.", Color.green);
            }
        }
    }

    private void ProcessContractsTick(System.DateTime time)
    {
        if (lastProcessedHour == -1)
        {
            lastProcessedHour = time.Hour;
            lastProcessedDay = time.DayOfYear;
            return;
        }

        bool hourChanged = (time.Hour != lastProcessedHour);
        bool dayChanged = (time.DayOfYear != lastProcessedDay);

        if (!hourChanged && !dayChanged) return;

        bool uiNeedsRefresh = false;

        foreach (Contract c in allContracts.ToList())
        {
            if (c.status != ContractStatus.Active) continue;

            ContractConfig config = ContractDatabase.GetConfig(c.contractType);
            bool wasOperatingHour = (lastProcessedHour >= config.StartHour && lastProcessedHour < config.EndHour);

            if (c.cycleType == ContractCycle.Hourly && hourChanged && wasOperatingHour)
            {
                EvaluateCycle(c, ref uiNeedsRefresh);
            }

            if (c.cycleType == ContractCycle.Daily && dayChanged)
            {
                EvaluateCycle(c, ref uiNeedsRefresh);
            }

            if (dayChanged)
            {
                c.daysRemaining--;
                uiNeedsRefresh = true;

                if (c.daysRemaining <= 0 && c.status == ContractStatus.Active)
                {
                    c.status = ContractStatus.Completed;

                    if (EconomyManager.Instance != null)
                        EconomyManager.Instance.AddMoney(c.completionBonus, $"Contract Completed: {c.contractType}");

                    if (PlayerScriptEngine.Instance != null)
                        PlayerScriptEngine.Instance.LogMessage($"CONTRACT FINISHED: {c.contractType} ended successfully! Bonus: {c.completionBonus} €", Color.yellow);
                }
            }
        }

        if (dayChanged)
        {
            RefreshMarket();
            uiNeedsRefresh = true;
        }

        lastProcessedHour = time.Hour;
        lastProcessedDay = time.DayOfYear;

        if (uiNeedsRefresh && ContractsUI.Instance != null && ContractsUI.Instance.IsWindowOpen())
        {
            ContractsUI.Instance.RefreshUI();
        }
    }

    private void EvaluateCycle(Contract c, ref bool uiNeedsRefresh)
    {
        if (c.deliveredInCurrentCycle >= c.targetMWhPerCycle)
        {
            c.deliveredInCurrentCycle = 0f;
            uiNeedsRefresh = true;
        }
        else
        {
            c.satisfaction -= 15f;

            if (EconomyManager.Instance != null)
                EconomyManager.Instance.TrySpendMoney(c.missedStepPenalty, $"Missed quota: {c.contractType}");

            if (PlayerScriptEngine.Instance != null)
                PlayerScriptEngine.Instance.LogMessage($"PENALTY: Missed {c.cycleType} quota for {c.contractType}. Fined {c.missedStepPenalty} €. Satisfaction dropped to {c.satisfaction}%.", Color.red);

            c.deliveredInCurrentCycle = 0f;
            uiNeedsRefresh = true;

            if (c.satisfaction <= 0)
            {
                c.satisfaction = 0;
                c.status = ContractStatus.Failed;

                if (EconomyManager.Instance != null)
                    EconomyManager.Instance.TrySpendMoney(c.failPenalty, $"Contract Terminated: {c.contractType}");

                if (PlayerScriptEngine.Instance != null)
                    PlayerScriptEngine.Instance.LogMessage($"CONTRACT TERMINATED: {c.contractType} cancelled due to 0% satisfaction! Massive fine: {c.failPenalty} €.", Color.red);
            }
        }
    }
}