using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central database holding all available contract configurations.
/// </summary>
public static class ContractDatabase
{
    // Constants for keys to avoid typos and magic strings
    public const string KeyHospital = "Hospital";
    public const string KeyDataCenter = "Data Center";
    public const string KeyHeavyIndustry = "Heavy Industry";
    public const string KeyLightIndustry = "Light Industry";
    public const string KeyCity = "City";

    // Dictionary linking the contract key to its full configuration
    public static readonly Dictionary<string, ContractConfig> Configs = new Dictionary<string, ContractConfig>
    {
        {
            KeyHospital,
            new ContractConfig(KeyHospital, ContractCycle.Hourly, 0, 24, "Critical infrastructure. Requires stable supply EVERY hour.")
        },
        {
            KeyDataCenter,
            new ContractConfig(KeyDataCenter, ContractCycle.Hourly, 0, 24, "High constant demand. Penalties for every hour of downtime.")
        },
        {
            KeyHeavyIndustry,
            new ContractConfig(KeyHeavyIndustry, ContractCycle.Hourly, 8, 16, "Demand during business hours only ({0}:00 - {1}:00).")
        },
        {
            KeyLightIndustry,
            new ContractConfig(KeyLightIndustry, ContractCycle.Hourly, 6, 20, "Moderate demand spread throughout the day ({0}:00 - {1}:00).")
        },
        {
            KeyCity,
            new ContractConfig(KeyCity, ContractCycle.Daily, 0, 24, "Daily quota. Must be fulfilled by midnight.")
        }
    };

    /// <summary>
    /// Safely retrieves a contract configuration by its key.
    /// </summary>
    public static ContractConfig GetConfig(string typeKey)
    {
        if (Configs.TryGetValue(typeKey, out ContractConfig config))
        {
            return config;
        }

        // Fallback to prevent null reference errors during runtime
        return new ContractConfig("Unknown", ContractCycle.Daily, 0, 24, "Standard contract.");
    }
}