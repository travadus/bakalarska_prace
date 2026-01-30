using System;

[Serializable]
public struct EnergyDataEntry
{
    public DateTime time;
    public float price;

    public EnergyDataEntry(DateTime time, float price)
    {
        this.time = time;
        this.price = price;
    }
}