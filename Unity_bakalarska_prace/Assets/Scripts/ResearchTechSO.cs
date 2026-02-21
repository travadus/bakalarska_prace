using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the data structure and configuration for a single technology node within the research tree.
/// </summary>
[CreateAssetMenu(fileName = "NewTech", menuName = "ScriptableObjects/Research")]
public class ResearchTechSO : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea] public string description;

    [Header("Economy")]
    public int researchCost;

    [Header("Progression")]
    public List<ResearchTechSO> prerequisites;

    /// <summary>
    /// Evaluates whether all required parent technologies have been successfully unlocked by the player.
    /// </summary>
    /// <returns>True if all prerequisite technologies are unlocked; otherwise, false.</returns>
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