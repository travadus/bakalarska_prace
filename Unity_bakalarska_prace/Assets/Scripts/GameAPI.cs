using UnityEngine;

public static class GameAPI
{
    public static MarketManager MarketSystem;
    public static EconomyManager EconomySystem => EconomyManager.Instance;

    public static void Trade()
    {
        Debug.Log("Provedena akce Obchod");
    }

    public static void Log(string message)
    {
        Debug.Log("Log: " + message);
    }

    public static void BuyEnergy(float amount)
    {
        Debug.Log("Nakoupena energie: " + amount + " MWh");
    }

    public static void SellEnergy(float amount)
    {
        Debug.Log("Prodána energie: " + amount + " MWh");
    }

    // Vrací aktuální cenu za 1 MWh
    public static float GetCurrentPrice()
    {
        if (MarketSystem != null)
        {
            return MarketSystem.GetCurrentPrice();
        }
        return 0f;
    }

    // Vrací kolik penìz má hráè na úètu
    public static float GetMoneyAmount()
    {
        if (EconomySystem != null) return EconomySystem.GetBalance();
        return 0f;
    }

    // Hráè mùže chtít v kódu zkontrolovat, jestli si mùže nìco dovolit
    public static bool CanAfford(float amount)
    {
        if (EconomySystem != null) return EconomySystem.GetBalance() >= amount;
        return false;
    }

    // Vrací pøedpovìï ceny na pøíští hodinu (aby hráè mohl plánovat)
    public static float GetPriceForecast()
    {
        return 0;
    }

    // Vrací aktuální spotøebu mìsta (Load), kterou musí hráè pokrýt
    public static float GetCityDemand()
    {
        return 0;
    }

    // Vrací aktuální výrobu ze všech zdrojù
    public static float GetCurrentProduction()
    {
        return 0;
    }

    // Vrací poèet postavených baterií (pro cykly)
    public static int GetBatteryCount()
    {
        return 0;
    }

    // Vrací nabití konkrétní baterie (0.0 až 1.0) - index je èíslo baterie
    public static float GetBatteryLevel(int index)
    {
        return 0;
    }

    // Vrací kapacitu baterie v MWh
    public static float GetBatteryCapacity(int index)
    {
        return 0;
    }

    // Nabit baterii
    public static void ChargeBattery(int index)
    {

    }

    //vzbit baterii
    public static void DischargeBattery(int index)
    {

    }

    // Nedìlá nic (standby)
    public static void StopBattery(int index)
    {

    }
}
