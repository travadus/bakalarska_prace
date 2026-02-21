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

    public float currentElectricityPricePerMWh = 0f;

    [SerializeField] private List<MoneyTransaction> transactionHistory = new List<MoneyTransaction>();

    public event Action<float> OnBalanceChanged;

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
    /// Adds a specified amount of money to the account balance.
    /// </summary>
    /// <param name="amount">The amount to add. Must be greater than zero.</param>
    /// <param name="description">A description of the income transaction.</param>
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
    /// Evaluates whether the account has sufficient funds to cover the specified cost.
    /// </summary>
    /// <param name="amount">The cost to evaluate.</param>
    /// <returns>True if the current balance is greater than or equal to the amount; otherwise, false.</returns>
    public bool CanAfford(float amount)
    {
        return currentBalance >= amount;
    }

    /// <summary>
    /// Attempts to deduct a specified amount from the account balance.
    /// </summary>
    /// <param name="amount">The amount to deduct. Must be greater than zero.</param>
    /// <param name="description">A description of the expense transaction.</param>
    /// <returns>True if the transaction was successful; false if there were insufficient funds.</returns>
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
    /// Forcefully deducts a specified amount from the account balance, allowing the balance to become negative.
    /// </summary>
    /// <param name="amount">The amount to deduct. Must be greater than zero.</param>
    /// <param name="description">A description of the penalty or expense transaction.</param>
    public void SubtractMoney(float amount, string description = "Penalty/Expense")
    {
        if (amount <= 0) return;

        currentBalance -= amount;
        LogTransaction(-amount, description);
        OnBalanceChanged?.Invoke(currentBalance);
    }

    // --- HELPER METHODS ---

    /// <summary>
    /// Creates a transaction record and adds it to the transaction history.
    /// </summary>
    /// <param name="amount">The transaction amount (positive for income, negative for expense).</param>
    /// <param name="description">A description of the transaction.</param>
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

    /// <summary>
    /// Retrieves the current account balance.
    /// </summary>
    /// <returns>The current balance as a float.</returns>
    public float GetBalance()
    {
        return currentBalance;
    }

    /// <summary>
    /// Retrieves the full history of financial transactions.
    /// </summary>
    /// <returns>A list of MoneyTransaction objects.</returns>
    public List<MoneyTransaction> GetHistory()
    {
        return transactionHistory;
    }

    /// <summary>
    /// Retrieves the current price of electricity per MWh.
    /// </summary>
    /// <returns>The electricity price as a float.</returns>
    public float GetCurrentElectricityPrice()
    {
        return currentElectricityPricePerMWh;
    }
}