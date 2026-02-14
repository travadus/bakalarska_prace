using UnityEngine;

/// <summary>
/// Defines the configuration and rules for a specific type of contract.
/// </summary>
public class ContractConfig
{
    public string TypeName { get; private set; }
    public ContractCycle Cycle { get; private set; }
    public int StartHour { get; private set; }
    public int EndHour { get; private set; }

    private string rawDescription;

    public ContractConfig(string typeName, ContractCycle cycle, int startHour, int endHour, string description)
    {
        TypeName = typeName;
        Cycle = cycle;
        StartHour = startHour;
        EndHour = endHour;
        rawDescription = description;
    }

    /// <summary>
    /// Returns the description with injected dynamic values (e.g., operating hours).
    /// </summary>
    public string GetFormattedDescription()
    {
        // {0} will be replaced by StartHour, {1} by EndHour
        return string.Format(rawDescription, StartHour, EndHour);
    }
}