using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchNode : MonoBehaviour
{
    [Header("Data")]
    // ZDE PØETÁHNI SCRIPTABLE OBJECT (napø. Tech_Variables)
    public ResearchTechSO techSO;

    [Header("Visual Connections")]
    // Sem ruènì pøetáhni tlaèítka rodièù (jen aby se vykreslily èáry)
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

    // Helper property to get ID easily
    public string TechID => techSO != null ? techSO.id : "";

    private void OnValidate()
    {
        // Automatické nastavení textù v editoru podle SO
        if (techSO != null)
        {
            gameObject.name = $"Node_{techSO.id}";
            if (labelText != null) labelText.text = techSO.displayName;
            if (costText != null) costText.text = $"{techSO.researchCost} RP";
        }
    }

    public void UpdateNodeState()
    {
        if (techSO == null || ResearchManager.Instance == null) return;

        // 1. Je už odemèeno?
        bool isUnlocked = ResearchManager.Instance.IsTechUnlocked(techSO.id);

        if (isUnlocked)
        {
            SetVisuals(colorUnlocked, false); // Už máme -> neklikatelné
            return;
        }

        // 2. Jsou splnìny podmínky (Rodièe)?
        // Ptáme se pøímo ScriptableObjectu, jestli jsou prerekvizity splnìny
        bool parentsMet = techSO.ArePrerequisitesMet();

        if (parentsMet)
        {
            // Mùžeme koupit
            SetVisuals(colorAvailable, true);
        }
        else
        {
            // Zamèeno
            SetVisuals(colorLocked, false);
        }
    }

    private void SetVisuals(Color c, bool interactable)
    {
        if (iconImage != null) iconImage.color = c;
        if (button != null) button.interactable = interactable;
    }
}