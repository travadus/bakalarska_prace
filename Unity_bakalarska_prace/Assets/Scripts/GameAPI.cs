using System;
using UnityEngine;

public static class GameAPI
{
    // --- REFERENCE ---
    public static MarketManager MarketSystem;
    public static EconomyManager EconomySystem => EconomyManager.Instance;

    // --- LOGOVÁNÍ ---
    public static event Action<string> OnLogMessage;

    /// <summary>
    /// Vypíše zprávu do herní konzole.
    /// Pøíklad: Log("Cena je: " + price);
    /// </summary>
    public static void Log(object message)
    {
        string msg = message != null ? message.ToString() : "null";
        OnLogMessage?.Invoke(msg);
    }

    // =================================================================================
    // SEKCE: TRH A PENÍZE (Market & Economy)
    // =================================================================================
    #region Market & Economy

    /// <summary>
    /// Nakoupí energii z globální sítì (Burzy) a pošle ji do lokální sbìrnice (Power Bus).
    /// Energie bude dostupná až v pøíštím herním tiku.
    /// </summary>
    public static void BuyEnergy(float amount)
    {
        if (amount <= 0) return;

        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.EnqueueAction(() =>
            {
                if (EconomyManager.Instance == null || EnergySystem.Instance == null)
                {
                    ReportError("System Error: Managers missing!");
                    return;
                }

                float price = EconomyManager.Instance.GetCurrentElectricityPrice();
                float totalCost = amount * price; // Mùže být záporné!

                // SCÉNÁØ A: Cena je kladná (Musíme platit)
                if (totalCost > 0)
                {
                    if (EconomyManager.Instance.TrySpendMoney(totalCost, $"Import: {amount} MWh"))
                    {
                        ProcessImport(amount);
                    }
                    else
                    {
                        PlayerScriptEngine.Instance.LogMessage("ERROR: Not enough money to buy energy!", Color.red);
                    }
                }
                // SCÉNÁØ B: Cena je záporná nebo nula (Dostaneme zaplaceno!)
                else
                {
                    // Pøièteme peníze (Math.Abs udìlá z -3 èíslo 3)
                    float gain = Mathf.Abs(totalCost);
                    EconomyManager.Instance.AddMoney(gain, $"Paid Import: {amount} MWh");

                    ProcessImport(amount);
                    PlayerScriptEngine.Instance.LogMessage($"PAID TO CONSUME: Received {gain:F2} € for importing {amount} MWh!", Color.cyan);
                }
            });
        }
    }

    // Pomocná metoda
    private static void ProcessImport(float amount)
    {
        EnergySystem.Instance.PlannedImport += amount;
        PlayerScriptEngine.Instance.LogMessage($"IMPORTED {amount} MWh to Power Bus", Color.green);
    }

    /// <summary>
    /// Okamžitì prodá energii (zatím spekulativní prodej).
    /// V budoucnu by to mìlo brát energii z pøebytkù sítì.
    /// </summary>
    public static void SellEnergy(float amount)
    {
        if (amount <= 0) return;

        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.EnqueueAction(() =>
            {
                if (EconomyManager.Instance == null) return;
                
                float price = EconomyManager.Instance.GetCurrentElectricityPrice();
                float totalRevenue = amount * price;

                // SCÉNÁØ A: Cena je kladná (Vydìláváme)
                if (totalRevenue > 0)
                {
                    EconomyManager.Instance.AddMoney(totalRevenue, $"Market Sell: {amount} MWh");
                    PlayerScriptEngine.Instance.LogMessage($"SOLD {amount} MWh for {totalRevenue:F2} €", Color.green);
                }
                // SCÉNÁØ B: Cena je záporná (Musíme platit za likvidaci!)
                else
                {
                    float penalty = Mathf.Abs(totalRevenue);
                    if (EconomyManager.Instance.TrySpendMoney(penalty, $"Market Dump: {amount} MWh"))
                    {
                        PlayerScriptEngine.Instance.LogMessage($"WARNING: Paid {penalty:F2} € to dump {amount} MWh (Negative Price)!", Color.yellow);
                    }
                    else
                    {
                        PlayerScriptEngine.Instance.LogMessage("ERROR: Not enough money to dump energy at negative price!", Color.red);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Vrátí aktuální tržní cenu za 1 MWh.
    /// </summary>
    public static float GetCurrentPrice()
    {
        if (MarketSystem != null) return MarketSystem.GetCurrentPrice();
        return 0f;
    }

    /// <summary>
    /// Vrátí aktuální zùstatek penìz na úètu hráèe.
    /// </summary>
    public static float GetMoneyAmount()
    {
        if (EconomySystem != null) return EconomySystem.GetBalance();
        return 0f;
    }

    // Pomocná metoda pro kontrolu, zda má hráè dost penìz
    public static bool CanAfford(float amount)
    {
        if (EconomySystem != null) return EconomySystem.GetBalance() >= amount;
        return false;
    }

    #endregion

    // =================================================================================
    // SEKCE: BATERIE (Battery Management)
    // =================================================================================
    #region Battery Management

    public static int GetBatteryCount()
    {
        if (BuildingsManager.Instance != null)
            return BuildingsManager.Instance.allBatteries.Count;
        return 0;
    }

    public static int[] GetBatteryIDs()
    {
        if (BuildingsManager.Instance != null)
        {
            int[] keys = new int[BuildingsManager.Instance.allBatteries.Count];
            BuildingsManager.Instance.allBatteries.Keys.CopyTo(keys, 0);
            return keys;
        }
        return new int[0];
    }

    /// <summary>
    /// Vrátí procento nabití baterie (0.0 až 1.0).
    /// Starý název: GetBatteryLevel
    /// </summary>
    public static float GetBatteryFillRatio(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            if (bat.maxStorageCapacity > 0)
                return bat.currentCharge / bat.maxStorageCapacity;
        }
        return 0f;
    }

    /// <summary>
    /// Vrátí pøesné množství uložené energie v MWh.
    /// Starý název: GetBatteryCharge
    /// </summary>
    public static float GetBatteryStoredMWh(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            return bat.currentCharge;
        }
        return 0f;
    }

    /// <summary>
    /// Vrátí maximální kapacitu baterie v MWh.
    /// </summary>
    public static float GetBatteryCapacity(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            return bat.maxStorageCapacity;
        }
        return 0f;
    }

    /// <summary>
    /// Pøepne baterii do režimu NABÍJENÍ (bere energii ze sítì).
    /// </summary>
    public static void ChargeBattery(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            bat.currentMode = BatteryMode.Charging;
        }
    }

    /// <summary>
    /// Pøepne baterii do režimu VYBÍJENÍ (posílá energii do sítì).
    /// </summary>
    public static void DischargeBattery(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            bat.currentMode = BatteryMode.Discharging;
        }
    }

    /// <summary>
    /// Vypne baterii (nebude dìlat nic).
    /// Starý název: StopBattery
    /// </summary>
    public static void SetBatteryStandby(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            bat.currentMode = BatteryMode.Standby;
        }
    }

    #endregion

    // =================================================================================
    // SEKCE: POÈASÍ A PØEDPOVÌÏ
    // =================================================================================
    #region Weather & Forecast

    // Vrátí aktuální data o poèasí
    public static float GetCurrentWind()
    {
        if (WeatherSystem.Instance != null) return WeatherSystem.Instance.CurrentWeather.WindIntensity;
        return 0f;
    }

    public static float GetCurrentSun()
    {
        if (WeatherSystem.Instance != null) return WeatherSystem.Instance.CurrentWeather.SunIntensity;
        return 0f;
    }

    public static float GetCurrentClouds()
    {
        if (WeatherSystem.Instance != null) return WeatherSystem.Instance.CurrentWeather.CloudDensity;
        return 0f;
    }

    // --- HLAVNÍ METODA PRO PØEDPOVÌÏ ---
    // hoursAhead = 0 (teï), 1 (za hodinu), 24 (zítra touto dobou)
    // Vrací objekt s daty, hráè si z nìj vytáhne, co potøebuje.
    // Protože GameAPI vrací primitivní typy lépe, rozdìlíme to na konkrétní dotazy:

    public static float GetForecastSun(int hoursAhead)
    {
        if (WeatherSystem.Instance == null || TimeSystem.Instance == null) return 0f;

        // 1. Zjistíme budoucí èas
        System.DateTime futureTime = TimeSystem.Instance.CurrentDateTime.AddHours(hoursAhead);

        // 2. Zeptáme se WeatherSystemu na poèasí v ten èas
        WeatherData forecast = WeatherSystem.Instance.CalculateWeatherForTime(futureTime);

        return forecast.SunIntensity;
    }

    public static float GetForecastWind(int hoursAhead)
    {
        if (WeatherSystem.Instance == null || TimeSystem.Instance == null) return 0f;
        System.DateTime futureTime = TimeSystem.Instance.CurrentDateTime.AddHours(hoursAhead);
        WeatherData forecast = WeatherSystem.Instance.CalculateWeatherForTime(futureTime);
        return forecast.WindIntensity;
    }

    #endregion

    // =================================================================================
    // SEKCE: INTERNÍ POMOCNÉ METODY (Helpers)
    // =================================================================================
    #region Helpers

    // Zkratka pro získání baterie a kontrolu chyb.
    // Díky "out" parametru vrací baterii pøímo, pokud existuje.
    private static bool TryGetBattery(int id, out BatteryBuilding battery)
    {
        battery = null;

        if (BuildingsManager.Instance == null) return false;

        if (BuildingsManager.Instance.allBatteries.TryGetValue(id, out battery))
        {
            return true; // Našli jsme
        }
        else
        {
            // Nenašli jsme -> Nahlásíme chybu
            ReportError($"Battery with ID {id} does not exist!");
            return false;
        }
    }

    private static void ReportError(string message)
    {
        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.LogMessage($"RUNTIME ERROR: {message}", Color.red);
        }
    }

    #endregion
}