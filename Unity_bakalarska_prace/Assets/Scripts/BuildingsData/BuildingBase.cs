using UnityEngine;

/// <summary>
/// Abstract base class defining the core identity and shared functionality for all placeable structures.
/// </summary>
public abstract class BuildingBase : MonoBehaviour
{
    public int id { get; private set; } = -1;
    public string BuildingName;

    /// <summary>
    /// Initializes the building with a unique identifier and updates its hierarchy representation.
    /// </summary>
    /// <param name="newID">The unique integer ID assigned by the building management system.</param>
    public void Setup(int newID)
    {
        this.id = newID;
        gameObject.name = $"{BuildingName}_{newID}";
    }

    /// <summary>
    /// Retrieves internal state data for debugging purposes.
    /// </summary>
    /// <returns>A formatted string containing debug information.</returns>
    public virtual string GetDebugInfo()
    {
        return "";
    }

    /// <summary>
    /// Retrieves the category or type name of the building for UI representation.
    /// </summary>
    /// <returns>The building type as a string.</returns>
    public virtual string GetBuildingType()
    {
        return "Unknown Building";
    }

    /// <summary>
    /// Retrieves the current operational status of the building.
    /// </summary>
    /// <returns>The status description as a string.</returns>
    public virtual string GetStatusText()
    {
        return "Active";
    }

    // --- TOOLTIP INTERACTIONS ---

    private void OnMouseEnter()
    {
        TooltipSystem.Instance.Show(GetTooltipContent(), GetTooltipHeader());
    }

    private void OnMouseExit()
    {
        TooltipSystem.Instance.Hide();
    }

    /// <summary>
    /// Continuously updates the tooltip content to reflect real-time value changes while the cursor remains on the object.
    /// </summary>
    private void OnMouseOver()
    {
        TooltipSystem.Instance.Show(GetTooltipContent(), GetTooltipHeader());
    }

    /// <summary>
    /// Defines the header text displayed within the building's tooltip.
    /// </summary>
    /// <returns>The tooltip header string.</returns>
    protected virtual string GetTooltipHeader()
    {
        return GetBuildingType();
    }

    /// <summary>
    /// Defines the body content displayed within the building's tooltip.
    /// </summary>
    /// <returns>The tooltip body string.</returns>
    protected virtual string GetTooltipContent()
    {
        return GetStatusText();
    }
}