using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A central static repository containing all predefined contract configurations.
/// </summary>
public static class ContractDatabase
{
    public const string KeyHospital = "Hospital";
    public const string KeyDataCenter = "Data Center";
    public const string KeyHeavyIndustry = "Heavy Industry";
    public const string KeyLightIndustry = "Light Industry";
    public const string KeyCity = "City";

    /// <summary>
    /// A read-only collection mapping unique contract keys to their respective configurations.
    /// </summary>
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
    /// Retrieves a specific contract configuration based on the provided key.
    /// </summary>
    /// <param name="typeKey">The unique identifier of the contract type.</param>
    /// <returns>The corresponding ContractConfig, or a default fallback if the key is not found.</returns>
    public static ContractConfig GetConfig(string typeKey)
    {
        if (Configs.TryGetValue(typeKey, out ContractConfig config))
        {
            return config;
        }

        // Returns a default fallback configuration to prevent null reference exceptions.
        return new ContractConfig("Unknown", ContractCycle.Daily, 0, 24, "Standard contract.");
    }
}