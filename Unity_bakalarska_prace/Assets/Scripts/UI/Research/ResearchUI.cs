using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
            // Dùležité: Lambda výraz pro pøedání konkrétního node
            node.button.onClick.AddListener(() => OnNodeClicked(node));
        }

        RefreshUI();
    }

    private void Update()
    {
        if (ResearchManager.Instance != null && rpDisplay != null)
        {
            rpDisplay.text = $"Research Points: {Mathf.FloorToInt(ResearchManager.Instance.CurrentResearchPoints)}";
        }
    }

    private void RefreshUI()
    {
        foreach (var node in allNodes)
        {
            node.UpdateNodeState();
        }
    }

    private void DrawConnections()
    {
        if (connectionManager == null) return;

        foreach (var node in allNodes)
        {
            // Kreslíme èáry podle tlaèítek, která jsi pøetáhl v Inspectoru
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

    public void OnNodeClicked(ResearchNode node)
    {
        if (ResearchManager.Instance == null || node.techSO == null) return;

        // --- TOTO JE TA OPRAVA ---
        // Manager sám zkontroluje peníze, prerekvizity a pokud vše klapne,
        // odeète body a odemkne technologii. Vrátí true/false.

        bool success = ResearchManager.Instance.TryUnlockTech(node.techSO);

        if (success)
        {
            // Refresh visuals immediately
            RefreshUI();

            // Volitelnì: Pøehrát zvuk
        }
        else
        {
            // Pokud se to nepovedlo (napø. málo bodù)
            if (PlayerScriptEngine.Instance != null)
                PlayerScriptEngine.Instance.LogMessage($"Cannot research {node.techSO.displayName}! Not enough points or locked.", Color.red);
        }
    }
}