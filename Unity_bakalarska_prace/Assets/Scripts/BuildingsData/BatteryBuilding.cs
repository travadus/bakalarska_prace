using UnityEngine;

// Dìdíme z BuildingBase (pro ID a jméno) a IGridActor (pro elektøinu)
public class BatteryBuilding : BuildingBase, IGridActor
{
    [Header("Nastavení (Edituj v Prefabu)")]
    public float maxStorageCapacity = 100f;
    public float maxInputOutput = 10f;

    [Header("Stav (Mìní se ve høe)")]
    // public int id;  <-- SMAZÁNO (Už je v BuildingBase)
    public float currentCharge = 0f;
    public BatteryMode currentMode = BatteryMode.Standby;

    private void Awake()
    {
        // DÙLEŽITÉ: Nastavíme jméno, které použije Manager pøi výpisu
        BuildingName = "Battery";

        currentCharge = 0f;
        currentMode = BatteryMode.Standby;
    }

    // public void Setup(int newID) <-- SMAZÁNO (Už je v BuildingBase)
    // Pokud potøebuješ resetovat náboj, udìlej to v Awake nebo Start.

    // Pøepíšeme metodu z BuildingBase, aby výpis v konzoli ukazoval kapacitu
    public override string GetDebugInfo()
    {
        return $"Capacity: {maxStorageCapacity} kWh, Charge: {currentCharge:F1}";
    }

    private void Start()
    {
        // 1. Registrace do Evidence (Genericky)
        if (BuildingsManager.Instance != null)
        {
            // Tady øíkáme: "Zaregistruj mì (this) do slovníku allBatteries"
            BuildingsManager.Instance.RegisterBuilding(this, BuildingsManager.Instance.allBatteries);
        }

        // 2. Registrace do Sítì (Elektøina)
        if (EnergySystem.Instance != null)
        {
            EnergySystem.Instance.RegisterActor(this);
        }
    }

    private void OnDestroy()
    {
        // Odhlášení
        if (BuildingsManager.Instance != null)
            BuildingsManager.Instance.UnregisterBuilding(this, BuildingsManager.Instance.allBatteries);

        if (EnergySystem.Instance != null)
            EnergySystem.Instance.UnregisterActor(this);
    }

    // --- IGridActor Implementace (Zùstává stejná) ---

    public float GetAvailableSupply()
    {
        if (currentMode == BatteryMode.Discharging)
        {
            return Mathf.Min(maxInputOutput, currentCharge);
        }
        return 0f;
    }

    public void ExtractEnergy(float amount)
    {
        currentCharge -= amount;
    }

    public float GetRequestedDemand()
    {
        if (currentMode == BatteryMode.Charging)
        {
            float spaceLeft = maxStorageCapacity - currentCharge;
            return Mathf.Min(maxInputOutput, spaceLeft);
        }
        return 0f;
    }

    public void ReceiveEnergy(float amount)
    {
        currentCharge += amount;
    }

    protected override string GetTooltipHeader()
    {
        return $"Battery Storage #{id}";
    }

    protected override string GetTooltipContent()
    {
        // Formátovaný string s barvièkama
        string status = currentMode.ToString();
        string color = "white";
        if (currentMode == BatteryMode.Charging) color = "green";
        if (currentMode == BatteryMode.Discharging) color = "red";

        return $"Energy: {currentCharge:F1} / {maxStorageCapacity} MWh\n" +
               $"Mode: <color={color}>{status}</color>";
    }
}