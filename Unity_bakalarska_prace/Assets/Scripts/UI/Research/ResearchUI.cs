using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Assembles the research tree user interface. 
/// Manages the collection of nodes, renders visual connections, 
/// and handles interaction between the UI and the research logic.
/// </summary>
public class ResearchUI : MonoBehaviour
{
    [Header("Managers")]
    public UIConnectionManager connectionManager;

    [Header("References")]
    public Transform nodesContainer;
    public TextMeshProUGUI rpDisplay;

    private List<ResearchNode> allNodes = new List<ResearchNode>();

    private void Start()
    {
        allNodes.AddRange(nodesContainer.GetComponentsInChildren<ResearchNode>());

        DrawConnections();

        foreach (var node in allNodes)
        {
            node.button.onClick.AddListener(() => OnNodeClicked(node));
        }

        RefreshUI();
    }

    /// <summary>
    /// Synchronizes the research points display with the current state.
    /// </summary>
    private void Update()
    {
        if (ResearchManager.Instance != null && rpDisplay != null)
        {
            rpDisplay.text = $"Research Points: {Mathf.FloorToInt(ResearchManager.Instance.CurrentResearchPoints)}";
        }
    }

    /// <summary>
    /// Triggers a state update for all registered research nodes.
    /// </summary>
    private void RefreshUI()
    {
        foreach (var node in allNodes)
        {
            node.UpdateNodeState();
        }
    }

    /// <summary>
    /// Renders visual connection lines between nodes and their assigned parents.
    /// </summary>
    private void DrawConnections()
    {
        if (connectionManager == null) return;

        foreach (var node in allNodes)
        {
            foreach (var parent in node.parentButtons)
            {
                if (parent != null)
                {
                    connectionManager.ConnectNodes(
                        parent.GetComponent<RectTransform>(),
                        node.GetComponent<RectTransform>(),
                        Color.white
                    );
                }
            }
        }
    }

    /// <summary>
    /// Handles node interaction by attempting to unlock the associated technology.
    /// </summary>
    /// <param name="node">The research node that was interacted with.</param>
    public void OnNodeClicked(ResearchNode node)
    {
        if (ResearchManager.Instance == null || node.techSO == null) return;

        bool success = ResearchManager.Instance.TryUnlockTech(node.techSO);

        if (success)
        {
            RefreshUI();
        }
        else
        {
            if (PlayerScriptEngine.Instance != null)
            {
                PlayerScriptEngine.Instance.LogMessage($"Cannot research {node.techSO.displayName}! Not enough points or locked.", Color.red);
            }
        }
    }
}