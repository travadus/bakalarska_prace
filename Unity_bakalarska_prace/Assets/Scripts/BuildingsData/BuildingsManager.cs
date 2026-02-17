using UnityEngine;
using System.Collections.Generic;

public class BuildingsManager : MonoBehaviour
{
    public static BuildingsManager Instance;

    // Slovník pro baterie (aby je GameAPI našlo)
    public Dictionary<int, BatteryBuilding> allBatteries = new Dictionary<int, BatteryBuilding>();

    public Dictionary<int, SolarBuilding> allSolars = new Dictionary<int, SolarBuilding>();

    public Dictionary<int, ResearchLab> allLabs = new Dictionary<int, ResearchLab>();

    // V budoucnu sem pøidáš tøeba:
    // public Dictionary<int, SolarBuilding> allSolars = ...

    private int nextId = 0;

    private void Awake()
    {
        Instance = this;
    }

    // --- GENERICKÁ REGISTRACE ---
    // Tuto metodu mùže volat Baterie, Solár, cokoliv co dìdí z BuildingBase
    public void RegisterBuilding<T>(T building, Dictionary<int, T> targetDictionary) where T : BuildingBase
    {
        // 1. Pøidìlíme ID
        int newID = nextId++;

        // 2. Zavoláme Setup na základní tøídì (BuildingBase)
        building.Setup(newID);

        // 3. Uložíme do správného slovníku
        targetDictionary.Add(newID, building);

        // 4. Logování
        if (PlayerScriptEngine.Instance != null)
        {
            // GetDebugInfo() si každá budova definuje po svém
            string extra = building.GetDebugInfo();
            PlayerScriptEngine.Instance.LogSystemMessage($"New {building.BuildingName} connected. ID: {newID}. {extra}");
        }
    }

    // --- GENERICKÉ ODHLÁŠENÍ ---
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