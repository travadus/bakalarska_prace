using UnityEngine;
using System;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour, ITickable
{
    public static MarketManager Instance { get; private set; }

    [Header("Data Source")]
    [SerializeField] private TextAsset csvFile;

    // Data uložená v pamìti
    private List<EnergyDataEntry> allMarketData;

    // Aktuálnì platná data
    private EnergyDataEntry currentData;
    private int lastSearchIndex = 0; // Optimalizace pro hledání

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
            // ZMÌNA 1: Místo "OnTick +=" se zaregistrujeme do seznamu
            TimeSystem.Instance.RegisterTickable(this);

            // ZMÌNA 2: Zavoláme OnTick manuálnì hned na zaèátku, 
            // aby se cena naèetla okamžitì po spuštìní hry (neèekáme na první tik).
            OnTick(TimeSystem.Instance.CurrentDateTime);
        }
    }

    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            // ZMÌNA 3: Slušnì se odhlásíme pøi znièení objektu
            TimeSystem.Instance.UnregisterTickable(this);
        }
    }

    public void OnTick(DateTime gameTime)
    {
        if (allMarketData == null || allMarketData.Count == 0) return;

        // Optimalizované hledání: Pokraèujeme tam, kde jsme skonèili minule
        for (int i = lastSearchIndex; i < allMarketData.Count; i++)
        {
            // Našli jsme pøesnou hodinu?
            if (IsSameHour(allMarketData[i].time, gameTime))
            {
                currentData = allMarketData[i];
                lastSearchIndex = i;
                return;
            }

            // Pokud jsme v datech "pøedbìhli" herní èas, zastavíme se 
            // a necháme platit poslední nalezenou cenu.
            if (allMarketData[i].time > gameTime)
            {
                break;
            }
        }
    }

    // --- Helpery a Public API ---

    private bool IsSameHour(DateTime dt1, DateTime dt2)
    {
        return dt1.Year == dt2.Year &&
               dt1.Month == dt2.Month &&
               dt1.Day == dt2.Day &&
               dt1.Hour == dt2.Hour;
    }

    public float GetCurrentPrice()
    {
        return currentData.price;
    }
}