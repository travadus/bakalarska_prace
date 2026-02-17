using System.Collections.Generic;
using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance { get; private set; }

    [Header("State")]
    public float CurrentResearchPoints = 0f;

    // Hashset is faster for lookups than List
    private HashSet<string> unlockedTechIDs = new HashSet<string>();

    [Header("Debug / Cheats")]
    public bool unlockAll = false; // For testing

    private void Awake()
    {
        Instance = this;
    }

    public void AddRP(float amount)
    {
        CurrentResearchPoints += amount;
        // Invoke UI update event here if needed
    }

    public bool TryUnlockTech(ResearchTechSO tech)
    {
        if (IsTechUnlocked(tech.id)) return false; // Already unlocked

        if (!tech.ArePrerequisitesMet()) return false; // Parents strictly required

        if (CurrentResearchPoints >= tech.researchCost)
        {
            CurrentResearchPoints -= tech.researchCost;
            unlockedTechIDs.Add(tech.id);

            if (PlayerScriptEngine.Instance != null)
                PlayerScriptEngine.Instance.LogSystemMessage($"Researched: {tech.displayName}");

            return true;
        }

        return false;
    }

    public bool IsTechUnlocked(string techID)
    {
        if (unlockAll) return true;
        return unlockedTechIDs.Contains(techID);
    }
}