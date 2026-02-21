using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages market data by parsing external CSV files and synchronizing in-game electricity prices 
/// with the global time system.
/// </summary>
public class MarketManager : MonoBehaviour, ITickable
{
    public static MarketManager Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private TextAsset csvFile;

    private List<EnergyDataEntry> allMarketData;
    private EnergyDataEntry currentData;
    private int lastSearchIndex = 0;

    private void Awake()
    {
        Instance = this;

        CsvMarketDataLoader loader = new CsvMarketDataLoader();
        allMarketData = loader.LoadData(csvFile);

        GameAPI.MarketSystem = this;
    }

    private void Start()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.RegisterTickable(this);

            // Manually trigger the first tick to initialize market prices immediately upon startup.
            OnTick(TimeSystem.Instance.CurrentDateTime);
        }
    }

    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.UnregisterTickable(this);
        }
    }

    /// <summary>
    /// Updates the current market data based on the provided game time.
    /// Utilizes an optimized linear search by caching the last valid index.
    /// </summary>
    /// <param name="gameTime">The current date and time in the simulation.</param>
    public void OnTick(DateTime gameTime)
    {
        if (allMarketData == null || allMarketData.Count == 0) return;

        for (int i = lastSearchIndex; i < allMarketData.Count; i++)
        {
            if (IsSameHour(allMarketData[i].time, gameTime))
            {
                currentData = allMarketData[i];
                lastSearchIndex = i;

                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.currentElectricityPricePerMWh = currentData.price;
                }

                return;
            }

            // Break early if the evaluated data entry is in the future.
            if (allMarketData[i].time > gameTime)
            {
                break;
            }
        }
    }

    // --- HELPERS ---

    /// <summary>
    /// Evaluates whether two DateTime objects represent the exact same hour of the same day.
    /// </summary>
    private bool IsSameHour(DateTime dt1, DateTime dt2)
    {
        return dt1.Year == dt2.Year &&
               dt1.Month == dt2.Month &&
               dt1.Day == dt2.Day &&
               dt1.Hour == dt2.Hour;
    }

    /// <summary>
    /// Retrieves the currently active market price for electricity.
    /// </summary>
    /// <returns>The current price per MWh as a float.</returns>
    public float GetCurrentPrice()
    {
        return currentData.price;
    }
}