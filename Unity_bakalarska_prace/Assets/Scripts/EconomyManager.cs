using System;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Nastavení")]
    [SerializeField] private float startBalance = 1000f; // Startovní kapitál

    [Header("Stav")]
    [SerializeField] private float currentBalance;

    // Historie všech plateb (pro grafy nebo výpis)
    [SerializeField] private List<MoneyTransaction> transactionHistory = new List<MoneyTransaction>();

    // Event, který oznámí zmìnu penìz (pro UI)
    public event Action<float> OnBalanceChanged;

    private void Awake()
    {
        Instance = this;
        currentBalance = startBalance;
    }

    private void Start()
    {
        // Inicializujeme UI hned na zaèátku
        OnBalanceChanged?.Invoke(currentBalance);
    }

    // --- HLAVNÍ METODY ---

    /// <summary>
    /// Pøidá peníze na úèet (Pøíjem)
    /// </summary>
    public void AddMoney(float amount, string description)
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
    /// Pokusí se utratit peníze. Pokud na to hráè nemá, vrátí false.
    /// </summary>
    public bool TrySpendMoney(float amount, string description)
    {
        if (amount <= 0) return false;

        if (currentBalance >= amount)
        {
            currentBalance -= amount;
            LogTransaction(-amount, description); // Ukládáme jako záporné èíslo

            OnBalanceChanged?.Invoke(currentBalance);
            return true;
        }
        else
        {
            Debug.Log("Insufficient funds for the transaction: " + description);
            return false;
        }
    }

    // Pomocná metoda pro zápis do historie
    private void LogTransaction(float amount, string description)
    {
        DateTime now = DateTime.MinValue;

        // Získáme aktuální herní èas, pokud existuje
        if (TimeSystem.Instance != null)
        {
            now = TimeSystem.Instance.CurrentDateTime;
        }

        MoneyTransaction t = new MoneyTransaction(now, amount, description, currentBalance);
        transactionHistory.Add(t);

        // Debug výpis
        Debug.Log($"TRANSACTION: {description} | {amount} EUR | Balance: {currentBalance}");
    }

    // --- Gettery ---
    public float GetBalance()
    {
        return currentBalance;
    }

    public List<MoneyTransaction> GetHistory()
    {
        return transactionHistory;
    }
}