using UnityEngine;

/// <summary>
/// Data model representing an individual contract.
/// </summary>
[System.Serializable]
public class Contract
{
    [Header("Basic Info")]
    public int id;
    public string contractType;
    public string tier;

    [Header("Status & Satisfaction")]
    public ContractStatus status;
    public float satisfaction = 100f;
    public int durationDays;
    public int daysRemaining;

    [Header("Economy")]
    public float rewardPerMWh;
    public float completionBonus;
    public float failPenalty;
    public float missedStepPenalty;

    [Header("Quota & Progress")]
    public ContractCycle cycleType;
    public float targetMWhPerCycle;
    public float deliveredInCurrentCycle;

    /// <summary>
    /// Calculates the remaining energy volume required to fulfill the current operational cycle's quota.
    /// </summary>
    /// <returns>The amount of MWh still needed, clamped to a minimum of zero.</returns>
    public float GetRemainingInCycle()
    {
        return Mathf.Max(0, targetMWhPerCycle - deliveredInCurrentCycle);
    }
}