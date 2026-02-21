using System;

/// <summary>
/// A data structure representing a single market record.
/// </summary>
[Serializable]
public struct EnergyDataEntry
{
    public DateTime time;
    public float price;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnergyDataEntry"/> structure.
    /// </summary>
    /// <param name="time">The timestamp of the record.</param>
    /// <param name="price">The electricity price at the given time.</param>
    public EnergyDataEntry(DateTime time, float price)
    {
        this.time = time;
        this.price = price;
    }
}