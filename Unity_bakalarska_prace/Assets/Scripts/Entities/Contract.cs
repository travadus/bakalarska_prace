using UnityEngine;

[System.Serializable]
public class Contract
{
    [Header("Basic Info")]
    public int id;
    public string contractType;      // Reference to ContractDatabase keys
    public string tier;              // "Standard", "VIP", "Urgent"

    [Header("Status & Satisfaction")]
    public ContractStatus status;
    public float satisfaction = 100f; // 0 to 100 %
    public int durationDays;          // Total duration of the contract in days
    public int daysRemaining;         // Days left until the contract expires

    [Header("Economy")]
    public float rewardPerMWh;        // Money earned for every delivered MWh immediately
    public float completionBonus;     // Bonus paid if contract finishes successfully
    public float failPenalty;         // Huge fine if contract is terminated (satisfaction <= 0)
    public float missedStepPenalty;   // Fine for every failed cycle (hour/day)

    [Header("Quota & Progress")]
    public ContractCycle cycleType;   // Checking frequency (Hourly / Daily)
    public float targetMWhPerCycle;   // Amount to deliver in the current cycle
    public float deliveredInCurrentCycle; // Amount already delivered in this cycle

    /// <summary>
    /// Returns the remaining energy needed to fulfill the current cycle's quota.
    /// </summary>
    public float GetRemainingInCycle()
    {
        return Mathf.Max(0, targetMWhPerCycle - deliveredInCurrentCycle);
    }
}