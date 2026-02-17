using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTech", menuName = "ScriptableObjects/Research")]
public class ResearchTechSO : ScriptableObject
{
    [Header("Identity")]
    public string id;               // Unikátní ID (napø. "tech_variables")
    public string displayName;      // Název pro UI (napø. "Variable Storage")
    [TextArea] public string description;

    [Header("Economy")]
    public int researchCost;        // Cena v RP

    [Header("Progression")]
    // Seznam technologií, které musí být odemèeny pøed touto
    public List<ResearchTechSO> prerequisites;

    /// <summary>
    /// Checks if all parent technologies are unlocked.
    /// </summary>
    public bool ArePrerequisitesMet()
    {
        if (ResearchManager.Instance == null) return false;

        foreach (var req in prerequisites)
        {
            if (!ResearchManager.Instance.IsTechUnlocked(req.id))
            {
                return false;
            }
        }
        return true;
    }
}