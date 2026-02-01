using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Nastavení")]
    [SerializeField] private float startBalance = 1000f; // Startovní kapitál

    [Header("Stav")]
    [SerializeField] private float currentBalance;

    // --- NOVÉ: Promìnná pro aktuální cenu elektøiny (z CSV) ---
    // Tvùj CSV Reader sem každou hodinu zapíše novou cenu.
    public float currentElectricityPricePerMWh = 0f;

    // Historie všech plateb
    [SerializeField] private List<MoneyTransaction> transactionHistory = new List<MoneyTransaction>();

    // Event, který oznámí zmìnu penìz (pro UI)
    public event Action<float> OnBalanceChanged;

    // --- NOVÉ: Vlastnost "Money" pro rychlý pøístup (zkratka) ---
    public float Money => currentBalance;

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
    /// Pokusí se utratit peníze. Pokud na to hráè nemá, vrátí false.
    /// (Použij pro nákup baterií, nabíjení atd.)
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
            // Debug.Log("Insufficient funds for: " + description);
            return false;
        }
    }

    /// <summary>
    /// --- NOVÉ ---
    /// Odeète peníze "natvrdo", i když jde hráè do mínusu.
    /// (Použij pro pokuty z kontraktù nebo pravidelné poplatky)
    /// </summary>
    public void SubtractMoney(float amount, string description = "Penalty/Expense")
    {
        if (amount <= 0) return;

        currentBalance -= amount;
        LogTransaction(-amount, description);
        OnBalanceChanged?.Invoke(currentBalance);
    }

    // --- POMOCNÉ METODY ---

    // Pomocná metoda pro zápis do historie
    private void LogTransaction(float amount, string description)
    {
        DateTime now = DateTime.MinValue;

        // Získáme aktuální herní èas, pokud existuje
        if (TimeSystem.Instance != null)
        {
            now = TimeSystem.Instance.CurrentDateTime;
        }

        // Tady pøedpokládám, že máš tøídu/strukturu MoneyTransaction definovanou jinde
        MoneyTransaction t = new MoneyTransaction(now, amount, description, currentBalance);
        transactionHistory.Add(t);

        // Debug.Log($"TRANSACTION: {description} | {amount} EUR | Balance: {currentBalance}");
    }

    // --- GETTERY (To, co ti chybìlo) ---

    public float GetBalance()
    {
        return currentBalance;
    }

    public List<MoneyTransaction> GetHistory()
    {
        return transactionHistory;
    }

    // --- NOVÉ: Metoda pro získání ceny elektøiny ---
    public float GetCurrentElectricityPrice()
    {
        return currentElectricityPricePerMWh;
    }
}