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

    // =================================================================================
    // SEKCE: TRH A PENÍZE (Market & Economy)
    // =================================================================================

    /// <summary>
    /// Nakoupí energii z globální sítì.
    /// OPRAVA: Už nepíše "IMPORTED", ale jen "BUY ORDER", protože energie dorazí až pøi Tiku.
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
                    if (PlayerScriptEngine.Instance != null)
                        PlayerScriptEngine.Instance.LogMessage("RUNTIME ERROR: Managers missing!", Color.red);
                    return;
                }

                float price = EconomyManager.Instance.GetCurrentElectricityPrice();
                float totalCost = amount * price;

                bool success = false;

                // SCÉNÁØ A: Cena je kladná (Musíme platit)
                if (totalCost > 0)
                {
                    if (EconomyManager.Instance.TrySpendMoney(totalCost, $"Order: {amount} MWh"))
                    {
                        success = true;
                    }
                    else
                    {
                        PlayerScriptEngine.Instance.LogMessage("ERROR: Not enough money to buy energy!", Color.red);
                    }
                }
                // SCÉNÁØ B: Cena je záporná (Dostaneme zaplaceno)
                else
                {
                    float gain = Mathf.Abs(totalCost);
                    EconomyManager.Instance.AddMoney(gain, $"Order Bonus: {amount} MWh");
                    PlayerScriptEngine.Instance.LogMessage($"PAID TO CONSUME: Received {gain:F2} € bonus.", Color.cyan);
                    success = true;
                }

                if (success)
                {
                    // 1. Zapíšeme do systému (dorazí pøíští Tik)
                    EnergySystem.Instance.PlannedImport += amount;

                    // 2. Hláška: JEN OBJEDNÁVKA (Žlutì)
                    // Hráè ví, že to je na cestì. Až to dorazí, EnergySystem napíše zelenì "GRID INPUT".
                    PlayerScriptEngine.Instance.LogMessage($"BUY ORDER: Waiting for {amount} MWh import...", Color.yellow);
                }
            });
        }
    }

    /// <summary>
    /// Zadá pøíkaz k prodeji energie.
    /// Samotný prodej probìhne až v EnergySystem.
    /// </summary>
    public static void SellEnergy(float amount)
    {
        if (amount <= 0) return;

        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.EnqueueAction(() =>
            {
                if (EnergySystem.Instance == null)
                {
                    if (PlayerScriptEngine.Instance != null)
                        PlayerScriptEngine.Instance.LogMessage("RUNTIME ERROR: EnergySystem not found!", Color.red);
                    return;
                }

                // Jen zapíšeme požadavek na export
                EnergySystem.Instance.PlannedExport += amount;

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
    // SECTION: CONTRACTS
    // =================================================================================
    #region Contracts

    /// <summary>
    /// Returns the remaining MWh required for the current cycle of the given contract.
    /// Returns 0 if contract is not active or not found.
    /// </summary>
    public static float GetContractRemainingQuota(int contractId)
    {
        if (ContractsManager.Instance == null) return 0f;

        Contract c = ContractsManager.Instance.allContracts.Find(x => x.id == contractId);
        if (c != null && c.status == ContractStatus.Active)
        {
            return c.GetRemainingInCycle();
        }
        return 0f;
    }

    /// <summary>
    /// Delivers energy from the grid bus to an active contract.
    /// </summary>
    public static void DeliverToContract(int contractId, float amount)
    {
        if (amount <= 0 || ContractsManager.Instance == null || EnergySystem.Instance == null || PlayerScriptEngine.Instance == null) return;

        PlayerScriptEngine.Instance.EnqueueAction(() =>
        {
            Contract c = ContractsManager.Instance.allContracts.Find(x => x.id == contractId);

            if (c == null)
            {
                PlayerScriptEngine.Instance.LogMessage($"ERROR: Contract {contractId} not found.", Color.red);
                return;
            }

            if (c.status != ContractStatus.Active)
            {
                PlayerScriptEngine.Instance.LogMessage($"ERROR: Contract {contractId} is not Active!", Color.red);
                return;
            }

            float availableEnergy = EnergySystem.Instance.PowerBusLevel;

            // SECURITY: Get exactly how much is missing for this cycle
            float energyNeeded = c.GetRemainingInCycle();
            float energyToDeliver = Mathf.Min(amount, energyNeeded);

            if (energyToDeliver <= 0)
            {
                PlayerScriptEngine.Instance.LogMessage($"Contract {contractId} quota is already full for this cycle.", Color.yellow);
                return;
            }

            if (availableEnergy >= energyToDeliver)
            {
                // Take energy from Grid
                EnergySystem.Instance.ConsumeEnergyFromBus(energyToDeliver);
                c.deliveredInCurrentCycle += energyToDeliver;

                // PAY THE PLAYER: In the new system, they get paid per MWh delivered
                float earnedMoney = energyToDeliver * c.rewardPerMWh;
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddMoney(earnedMoney, $"Delivery to {c.contractType}");
                }

                PlayerScriptEngine.Instance.LogMessage($"DELIVERED: {energyToDeliver:F1} MWh to {c.contractType}. Earned: {earnedMoney:F1} €", Color.green);

                if (ContractsUI.Instance != null && ContractsUI.Instance.IsWindowOpen())
                {
                    ContractsUI.Instance.RefreshUI();
                }
            }
            else
            {
                PlayerScriptEngine.Instance.LogMessage($"DELIVERY FAILED: Not enough energy in Grid Bus. (Requested {energyToDeliver:F1}, Available {availableEnergy:F1})", Color.red);
            }
        });
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
    // SEKCE: SOLÁRNÍ PANELY (Solar Management)
    // =================================================================================
    #region Solar Management

    public static int GetSolarCount()
    {
        if (BuildingsManager.Instance != null)
            return BuildingsManager.Instance.allSolars.Count;
        return 0;
    }

    public static int[] GetSolarIDs()
    {
        if (BuildingsManager.Instance != null)
        {
            int[] keys = new int[BuildingsManager.Instance.allSolars.Count];
            BuildingsManager.Instance.allSolars.Keys.CopyTo(keys, 0);
            return keys;
        }
        return new int[0];
    }

    /// <summary>
    /// Vrátí aktuální výrobu panelu v MWh (už po odeètení špíny a mrakù).
    /// </summary>
    public static float GetSolarOutput(int id)
    {
        if (TryGetSolar(id, out var solar))
        {
            return solar.CurrentProduction;
        }
        return 0f;
    }

    /// <summary>
    /// Vrátí zneèištìní panelu (0.00 až 1.00). 
    /// 0.00 = Èistý, 1.00 = Špinavý (nevyrobí nic).
    /// </summary>
    public static float GetSolarDirtLevel(int id)
    {
        if (TryGetSolar(id, out var solar))
        {
            return solar.dirtLevel;
        }
        return 0f;
    }

    /// <summary>
    /// Zaplatí úklidové èety, aby panel vyèistily.
    /// </summary>
    public static void CleanSolarPanel(int id)
    {
        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.EnqueueAction(() =>
            {
                if (TryGetSolar(id, out var solar))
                {
                    float cost = solar.cleaningCost;

                    if (EconomyManager.Instance.TrySpendMoney(cost, $"Cleaning Solar {id}"))
                    {
                        solar.CleanPanels();
                        PlayerScriptEngine.Instance.LogMessage($"MAINTENANCE: Solar {id} cleaned for {cost} €.", Color.cyan);
                    }
                    else
                    {
                        PlayerScriptEngine.Instance.LogMessage("ERROR: Not enough money to clean solar panel!", Color.red);
                    }
                }
            });
        }
    }

    // --- Helper pro Soláry ---
    private static bool TryGetSolar(int id, out SolarBuilding solar)
    {
        solar = null;
        if (BuildingsManager.Instance == null) return false;
        if (BuildingsManager.Instance.allSolars.TryGetValue(id, out solar)) return true;

        ReportError($"Solar with ID {id} does not exist!");
        return false;
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