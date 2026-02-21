using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Management system for registering, tracking, and unregistering all placeable buildings.
/// </summary>
public class BuildingsManager : MonoBehaviour
{
    public static BuildingsManager Instance;

    public Dictionary<int, BatteryBuilding> allBatteries = new Dictionary<int, BatteryBuilding>();
    public Dictionary<int, SolarBuilding> allSolars = new Dictionary<int, SolarBuilding>();
    public Dictionary<int, ResearchLab> allResearchLabs = new Dictionary<int, ResearchLab>();

    private int nextId = 0;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Generically registers a new building into the management system, assigns it a unique ID, 
    /// and stores it in the designated type-specific dictionary.
    /// </summary>
    /// <typeparam name="T">The specific type of the building.</typeparam>
    /// <param name="building">The building instance to register.</param>
    /// <param name="targetDictionary">The corresponding dictionary where the building will be stored.</param>
    public void RegisterBuilding<T>(T building, Dictionary<int, T> targetDictionary) where T : BuildingBase
    {
        int newID = nextId++;

        building.Setup(newID);

        targetDictionary.Add(newID, building);

        if (PlayerScriptEngine.Instance != null)
        {
            string extra = building.GetDebugInfo();
            PlayerScriptEngine.Instance.LogSystemMessage($"New {building.BuildingName} connected. ID: {newID}. {extra}");
        }
    }

    /// <summary>
    /// Generically unregisters an existing building from the management system and removes it from its respective dictionary.
    /// </summary>
    /// <typeparam name="T">The specific type of the building.</typeparam>
    /// <param name="building">The building instance to unregister.</param>
    /// <param name="targetDictionary">The dictionary from which the building should be removed.</param>
    public void UnregisterBuilding<T>(T building, Dictionary<int, T> targetDictionary) where T : BuildingBase
    {
        if (targetDictionary.ContainsKey(building.id))
        {
            targetDictionary.Remove(building.id);

            if (PlayerScriptEngine.Instance != null)
            {
                PlayerScriptEngine.Instance.LogSystemMessage($"{building.BuildingName} ID: {building.id} signal lost (Destroyed).");
            }
        }
    }
}