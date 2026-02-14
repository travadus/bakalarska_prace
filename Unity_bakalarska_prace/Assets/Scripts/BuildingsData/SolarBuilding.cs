using UnityEngine;

public class SolarBuilding : BuildingBase, IGridActor
{
    [Header("Parametry (Edituj v Prefabu)")]
    public float maxOutput = 5.0f; // Maximální výroba (MWh) pøi 100% slunci a èistotì
    public float cleaningCost = 50f; // Cena za vyèištìní

    [Header("Rychlost špinìní")]
    // Kolik špíny pøibude za jednu hodinu (0.0002 = 0.02%)
    public float dustRate = 0.0002f;

    [Header("Stav (Mìní se ve høe)")]
    [Range(0, 1)]
    public float dirtLevel = 0f; // 0 = èistý, 1 = totálnì špinavý (0% výroba)

    // Veøejná vlastnost, aby si ji mohlo pøeèíst GameAPI
    public float CurrentProduction { get; private set; }

    private void Awake()
    {
        BuildingName = "Solar Farm";
    }

    private void Start()
    {
        // 1. Registrace do Evidence (BuildingsManager)
        if (BuildingsManager.Instance != null)
        {
            // Použijeme tvou novou generickou metodu!
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allSolars);
        }

        // 2. Registrace do Sítì (EnergySystem) - aby posílal elektøinu
        if (EnergySystem.Instance != null)
        {
            EnergySystem.Instance.RegisterActor(this);
        }
    }

    private void OnDestroy()
    {
        // Odhlášení pøi znièení
        if (BuildingsManager.Instance != null)
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allSolars);

        if (EnergySystem.Instance != null)
            EnergySystem.Instance.UnregisterActor(this);
    }

    // --- IGridActor: LOGIKA VÝROBY ---

    public float GetAvailableSupply()
    {
        // 1. Zjistíme slunce
        float sunIntensity = 0f;
        if (WeatherSystem.Instance != null)
        {
            sunIntensity = WeatherSystem.Instance.CurrentWeather.SunIntensity;
        }

        // 2. Aplikujeme špínu
        float efficiency = 1.0f - dirtLevel;
        if (efficiency < 0) efficiency = 0;

        // 3. Výpoèet výroby
        CurrentProduction = maxOutput * sunIntensity * efficiency;

        // 4. Simulace špinìní (OPRAVA: Dìje se to vždy, i v noci)
        // Odstranili jsme "if (sunIntensity > 0)"

        // Použijeme tvou nastavitelnou promìnnou dustRate
        // (Pokud ji nemáš, použij pevné èíslo napø. 0.0001f)
        float change = dustRate * Random.Range(0.8f, 1.2f);
        dirtLevel += change;

        // Zarážka, aby to nešlo pøes 100%
        if (dirtLevel > 1.0f) dirtLevel = 1.0f;

        return CurrentProduction;
    }

    // Solár energii jen dává, nikdy ji nebere (ExtractEnergy jen potvrdí odbìr)
    public void ExtractEnergy(float amount) { }

    // Solár nic nespotøebovává
    public float GetRequestedDemand() => 0f;
    public void ReceiveEnergy(float amount) { }

    // --- ÚDRŽBA ---

    public void CleanPanels()
    {
        dirtLevel = 0f; // Lesk jako blesk
    }

    // Pro výpis do konzole pøi startu
    public override string GetDebugInfo()
    {
        return $"Max Output: {maxOutput} MWh";
    }
}