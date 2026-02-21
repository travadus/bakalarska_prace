using System;

public interface IGridActor
{
    // --- SUPPLY (PRODUCER) ---

    /// <summary> Returns currently available supply in MWh. </summary>
    float GetAvailableSupply();

    /// <summary> Finalizes energy extraction from the actor. </summary>
    void ExtractEnergy(float amount);

    // --- DEMAND (CONSUMER) ---

    /// <summary> Returns requested energy demand in MWh. </summary>
    float GetRequestedDemand();

    /// <summary> Delivers allocated energy to the actor. </summary>
    void ReceiveEnergy(float amount);
}