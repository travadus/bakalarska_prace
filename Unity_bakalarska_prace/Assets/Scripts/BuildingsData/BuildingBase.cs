using UnityEngine;

// Abstract base class for all buildings in the game
public abstract class BuildingBase : MonoBehaviour
{
    public int id { get; private set; } = -1; // -1 means not assigned yet
    public string BuildingName; // E.g., "Battery", "Solar Panel"

    // Called by BuildingsManager when registering the building
    public void Setup(int newID)
    {
        this.id = newID;
        // Rename gameObject in Hierarchy for better organization
        gameObject.name = $"{BuildingName}_{newID}";
    }

    // --- Virtual Methods (Children can override these) ---

    /// <summary>
    /// Returns debug information for the console logs.
    /// </summary>
    public virtual string GetDebugInfo()
    {
        return ""; // Default: No extra info
    }

    /// <summary>
    /// Returns the category/type of the building (Used in UI).
    /// </summary>
    public virtual string GetBuildingType()
    {
        return "Unknown Building";
    }

    /// <summary>
    /// Returns the current status description (e.g., "Charging", "Paused").
    /// </summary>
    public virtual string GetStatusText()
    {
        return "Active";
    }
}