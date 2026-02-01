using UnityEngine;

// Třída, ze které budou dědit všechny budovy ve hře
public abstract class BuildingBase : MonoBehaviour
{
    public int id { get; private set; } = -1; // -1 znamená zatím nepřiřazeno
    public string BuildingName; // Např. "Battery", "Solar Panel"

    // Tuto metodu zavolá Manager při registraci
    public void Setup(int newID)
    {
        this.id = newID;
        // Můžeš sem přidat třeba změnu názvu objektu v hierarchii pro přehlednost
        gameObject.name = $"{BuildingName}_{newID}";
    }

    // Virtuální metoda pro logování (každá budova si ji může přepsat po svém)
    public virtual string GetDebugInfo()
    {
        return ""; // Základní budova nemá žádné extra info
    }
}