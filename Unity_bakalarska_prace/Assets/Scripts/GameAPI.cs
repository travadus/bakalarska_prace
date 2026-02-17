using System;
using UnityEngine;

public static class GameAPI
{
    // --- REFERENCES ---
    public static MarketManager MarketSystem;
    public static EconomyManager EconomySystem => EconomyManager.Instance;

    // --- LOGGING EVENTS ---
    public static event Action<string> OnLogMessage;

    /// <summary>
    /// Prints a message to the in-game console.
    /// Example: Log("Price is: " + price);
    /// </summary>
    public static void Log(object message)
    {
        string msg = message != null ? message.ToString() : "null";
        OnLogMessage?.Invoke(msg);
    }

    // =================================================================================
    // SECTION: MARKET & ECONOMY
    // =================================================================================
    #region Market & Economy

    /// <summary>
    /// Purchases energy from the global grid.
    /// Note: Energy arrives at the next Tick.
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

                // SCENARIO A: Price is positive (Player pays money)
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
                // SCENARIO B: Price is negative (Player gets paid to consume)
                else
                {
                    float gain = Mathf.Abs(totalCost);
                    EconomyManager.Instance.AddMoney(gain, $"Order Bonus: {amount} MWh");
                    PlayerScriptEngine.Instance.LogMessage($"PAID TO CONSUME: Received {gain:F2} € bonus.", Color.cyan);
                    success = true;
                }

                if (success)
                {
                    // 1. Register import (arrives next tick)
                    EnergySystem.Instance.PlannedImport += amount;

                    // 2. User Feedback
                    PlayerScriptEngine.Instance.LogMessage($"BUY ORDER: Waiting for {amount} MWh import...", Color.yellow);
                }
            });
        }
    }

    /// <summary>
    /// Submits an order to sell energy.
    /// The actual sale happens in the EnergySystem during the next Tick.
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

                // Register export request
                EnergySystem.Instance.PlannedExport += amount;
            });
        }
    }

    /// <summary>
    /// Returns the current market price for 1 MWh.
    /// </summary>
    public static float GetCurrentPrice()
    {
        if (MarketSystem != null) return MarketSystem.GetCurrentPrice();
        return 0f;
    }

    /// <summary>
    /// Returns the current balance of the player's account.
    /// </summary>
    public static float GetMoneyAmount()
    {
        if (EconomySystem != null) return EconomySystem.GetBalance();
        return 0f;
    }

    /// <summary>
    /// Helper method to check if the player can afford a specific amount.
    /// </summary>
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

                // PAY THE PLAYER: Payment happens per MWh delivered
                float earnedMoney = energyToDeliver * c.rewardPerMWh;
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddMoney(earnedMoney, $"Delivery to {c.contractType}");
                }

                PlayerScriptEngine.Instance.LogMessage($"DELIVERED: {energyToDeliver:F1} MWh to {c.contractType}. Earned: {earnedMoney:F1} €", Color.green);

                // Update UI if open
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
    // SECTION: BATTERY MANAGEMENT
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
    /// Returns battery charge percentage (0.0 to 1.0).
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
    /// Returns exact amount of stored energy in MWh.
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
    /// Returns maximum capacity of the battery in MWh.
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
    /// Sets battery to CHARGING mode (draws energy from grid).
    /// </summary>
    public static void ChargeBattery(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            bat.currentMode = BatteryMode.Charging;
        }
    }

    /// <summary>
    /// Sets battery to DISCHARGING mode (sends energy to grid).
    /// </summary>
    public static void DischargeBattery(int id)
    {
        if (TryGetBattery(id, out var bat))
        {
            bat.currentMode = BatteryMode.Discharging;
        }
    }

    /// <summary>
    /// Sets battery to STANDBY mode (does nothing).
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
    // SECTION: SOLAR MANAGEMENT
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
    /// Returns current output of the panel in MWh (after dirt and clouds calculation).
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
    /// Returns dirt level (0.00 to 1.00).
    /// 0.00 = Clean, 1.00 = Dirty (zero production).
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
    /// Pays maintenance crew to clean the solar panel.
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

    #endregion

    // =================================================================================
    // SECTION: RESEARCH LAB MANAGEMENT (NEW)
    // =================================================================================
    #region Research Management

    public static int GetResearchLabCount()
    {
        if (BuildingsManager.Instance != null)
            return BuildingsManager.Instance.allResearchLabs.Count;
        return 0;
    }

    public static int[] GetResearchLabIDs()
    {
        if (BuildingsManager.Instance != null)
        {
            int[] keys = new int[BuildingsManager.Instance.allResearchLabs.Count];
            BuildingsManager.Instance.allResearchLabs.Keys.CopyTo(keys, 0);
            return keys;
        }
        return new int[0];
    }

    /// <summary>
    /// Turns a specific Research Lab ON or OFF.
    /// If active, it consumes money and generates Research Points (RP) every tick.
    /// </summary>
    public static void SetResearchLabState(int id, bool active)
    {
        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.EnqueueAction(() =>
            {
                if (TryGetResearchLab(id, out var lab))
                {
                    // Check if state is actually changing to avoid spamming logs
                    if (lab.isOperating != active)
                    {
                        lab.isOperating = active;
                        string status = active ? "ONLINE" : "OFFLINE";
                        Color c = active ? Color.green : Color.orange;
                        PlayerScriptEngine.Instance.LogMessage($"LAB {id}: Systems {status}.", c);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Checks if a Research Lab is currently active.
    /// </summary>
    public static bool IsResearchLabActive(int id)
    {
        if (TryGetResearchLab(id, out var lab))
        {
            return lab.isOperating;
        }
        return false;
    }

    #endregion

    // =================================================================================
    // SECTION: WEATHER & FORECAST
    // =================================================================================
    #region Weather & Forecast

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

    /// <summary>
    /// Returns forecasted sun intensity for hours ahead (0-24).
    /// </summary>
    public static float GetForecastSun(int hoursAhead)
    {
        if (WeatherSystem.Instance == null || TimeSystem.Instance == null) return 0f;

        System.DateTime futureTime = TimeSystem.Instance.CurrentDateTime.AddHours(hoursAhead);
        WeatherData forecast = WeatherSystem.Instance.CalculateWeatherForTime(futureTime);

        return forecast.SunIntensity;
    }

    /// <summary>
    /// Returns forecasted wind intensity for hours ahead (0-24).
    /// </summary>
    public static float GetForecastWind(int hoursAhead)
    {
        if (WeatherSystem.Instance == null || TimeSystem.Instance == null) return 0f;

        System.DateTime futureTime = TimeSystem.Instance.CurrentDateTime.AddHours(hoursAhead);
        WeatherData forecast = WeatherSystem.Instance.CalculateWeatherForTime(futureTime);

        return forecast.WindIntensity;
    }

    #endregion

    // =================================================================================
    // SECTION: INTERNAL HELPERS
    // =================================================================================
    #region Helpers

    private static bool TryGetBattery(int id, out BatteryBuilding battery)
    {
        battery = null;
        if (BuildingsManager.Instance == null) return false;

        if (BuildingsManager.Instance.allBatteries.TryGetValue(id, out battery))
        {
            return true;
        }
        else
        {
            ReportError($"Battery with ID {id} does not exist!");
            return false;
        }
    }

    private static bool TryGetSolar(int id, out SolarBuilding solar)
    {
        solar = null;
        if (BuildingsManager.Instance == null) return false;

        if (BuildingsManager.Instance.allSolars.TryGetValue(id, out solar))
        {
            return true;
        }
        else
        {
            ReportError($"Solar with ID {id} does not exist!");
            return false;
        }
    }

    private static bool TryGetResearchLab(int id, out ResearchLab lab)
    {
        lab = null;
        if (BuildingsManager.Instance == null) return false;

        // Assuming 'allResearchLabs' dictionary exists in BuildingsManager
        if (BuildingsManager.Instance.allResearchLabs.TryGetValue(id, out lab))
        {
            return true;
        }
        else
        {
            ReportError($"Research Lab with ID {id} does not exist!");
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