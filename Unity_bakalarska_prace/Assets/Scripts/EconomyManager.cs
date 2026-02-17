using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float startBalance = 1000f;

    [Header("Status")]
    [SerializeField] private float currentBalance;

    // Current electricity price per MWh (updated from CSV)
    public float currentElectricityPricePerMWh = 0f;

    // Transaction history
    [SerializeField] private List<MoneyTransaction> transactionHistory = new List<MoneyTransaction>();

    // UI Event
    public event Action<float> OnBalanceChanged;

    // Property shortcut
    public float Money => currentBalance;

    private void Awake()
    {
        Instance = this;
        currentBalance = startBalance;
    }

    private void Start()
    {
        OnBalanceChanged?.Invoke(currentBalance);
    }

    // --- MAIN METHODS ---

    /// <summary>
    /// Adds money to the account (Income).
    /// </summary>
    public void AddMoney(float amount, string description = "Income")
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Attempt to add a negative or zero amount. Use SpendMoney.");
            return;
        }

        currentBalance += amount;
        LogTransaction(amount, description);
        OnBalanceChanged?.Invoke(currentBalance);
    }

    /// <summary>
    /// --- PØIDÁNO: ZJIŠTÌNÍ DOSTUPNOSTI PROSTØEDKÙ ---
    /// Checks if the player can afford a certain cost without spending it yet.
    /// </summary>
    public bool CanAfford(float amount)
    {
        return currentBalance >= amount;
    }

    /// <summary>
    /// Attempts to spend money. Returns true if successful, false if not enough money.
    /// </summary>
    public bool TrySpendMoney(float amount, string description)
    {
        if (amount <= 0) return false;

        if (currentBalance >= amount)
        {
            currentBalance -= amount;
            LogTransaction(-amount, description); // Store as negative
            OnBalanceChanged?.Invoke(currentBalance);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Forcefully subtracts money, even if balance goes negative (Penalties).
    /// </summary>
    public void SubtractMoney(float amount, string description = "Penalty/Expense")
    {
        if (amount <= 0) return;

        currentBalance -= amount;
        LogTransaction(-amount, description);
        OnBalanceChanged?.Invoke(currentBalance);
    }

    // --- HELPER METHODS ---

    private void LogTransaction(float amount, string description)
    {
        DateTime now = DateTime.MinValue;

        if (TimeSystem.Instance != null)
        {
            now = TimeSystem.Instance.CurrentDateTime;
        }

        MoneyTransaction t = new MoneyTransaction(now, amount, description, currentBalance);
        transactionHistory.Add(t);
    }

    // --- GETTERS ---

    public float GetBalance()
    {
        return currentBalance;
    }

    public List<MoneyTransaction> GetHistory()
    {
        return transactionHistory;
    }

    public float GetCurrentElectricityPrice()
    {
        return currentElectricityPricePerMWh;
    }
}