using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's research progression, including the accumulation of research points
/// and the unlocking of technologies within the tech tree.
/// </summary>
public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance { get; private set; }

    [Header("State")]
    public float CurrentResearchPoints = 0f;

    /// <summary>
    /// Stores the IDs of all currently unlocked technologies. 
    /// </summary>
    private HashSet<string> unlockedTechIDs = new HashSet<string>();

    [Header("Debug / Cheats")]
    public bool unlockAll = false;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Adds a specified amount of research points (RP) to the current total.
    /// </summary>
    /// <param name="amount">The amount of research points to add.</param>
    public void AddRP(float amount)
    {
        CurrentResearchPoints += amount;
    }

    /// <summary>
    /// Attempts to unlock a specific technology by validating its current state, 
    /// required prerequisites, and the availability of sufficient research points.
    /// </summary>
    /// <param name="tech">The technology configuration to be unlocked.</param>
    /// <returns>True if the technology was successfully unlocked; otherwise, false.</returns>
    public bool TryUnlockTech(ResearchTechSO tech)
    {
        if (IsTechUnlocked(tech.id)) return false;

        if (!tech.ArePrerequisitesMet()) return false;

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

    /// <summary>
    /// Checks whether a specific technology is currently unlocked.
    /// Safely handles debug overrides for testing purposes.
    /// </summary>
    /// <param name="techID">The unique identifier of the technology.</param>
    /// <returns>True if the technology is unlocked or if the debug override is active.</returns>
    public bool IsTechUnlocked(string techID)
    {
        if (unlockAll) return true;
        return unlockedTechIDs.Contains(techID);
    }
}