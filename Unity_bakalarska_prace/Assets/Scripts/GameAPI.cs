using UnityEngine;

public static class GameAPI
{
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
}
