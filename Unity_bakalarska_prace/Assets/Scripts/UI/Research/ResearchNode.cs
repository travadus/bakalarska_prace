using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a node within the research tree. 
/// </summary>
public class ResearchNode : MonoBehaviour
{
    [Header("Data")]
    public ResearchTechSO techSO;

    [Header("Visual Connections")]
    public List<ResearchNode> parentButtons;

    [Header("UI References")]
    public Button button;
    public TextMeshProUGUI labelText;
    public TextMeshProUGUI costText;
    public Image iconImage;

    [Header("Colors")]
    public Color colorLocked = Color.gray;
    public Color colorAvailable = Color.yellow;
    public Color colorUnlocked = Color.green;

    /// <summary>
    /// Gets the unique identifier of the associated technology.
    /// </summary>
    public string TechID => techSO != null ? techSO.id : "";

    /// <summary>
    /// Synchronizes UI elements with the ScriptableObject data.
    /// </summary>
    private void OnValidate()
    {
        if (techSO != null)
        {
            gameObject.name = $"Node_{techSO.id}";
            if (labelText != null) labelText.text = techSO.displayName;
            if (costText != null) costText.text = $"{techSO.researchCost} RP";
        }
    }

    /// <summary>
    /// Evaluates the current state of the technology
    /// and updates the node's visual representation and interactivity.
    /// </summary>
    public void UpdateNodeState()
    {
        if (techSO == null || ResearchManager.Instance == null) return;

        // Check if the technology is already researched
        bool isUnlocked = ResearchManager.Instance.IsTechUnlocked(techSO.id);

        if (isUnlocked)
        {
            SetVisuals(colorUnlocked, false);
            return;
        }

        bool parentsMet = techSO.ArePrerequisitesMet();

        if (parentsMet)
        {
            SetVisuals(colorAvailable, true);
        }
        else
        {
            SetVisuals(colorLocked, false);
        }
    }

    /// <summary>
    /// Updates the node's color and button interactability.
    /// </summary>
    /// <param name="c">The target color for the icon.</param>
    /// <param name="interactable">Whether the research button should be clickable.</param>
    private void SetVisuals(Color c, bool interactable)
    {
        if (iconImage != null) iconImage.color = c;
        if (button != null) button.interactable = interactable;
    }
}