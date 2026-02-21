using System;

/// <summary> Interface for systems requiring synchronized ticks. </summary>
public interface ITickable
{
    /// <summary> Executed on every game tick. </summary>
    void OnTick(DateTime currentDateTime);
}