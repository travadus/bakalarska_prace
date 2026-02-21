using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the contracts user interface.
/// </summary>
public class ContractsUI : MonoBehaviour
{
    public static ContractsUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject windowPanel;
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject contractPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (windowPanel != null) windowPanel.SetActive(false);
    }

    /// <summary>
    /// Toggles the visibility of the contract window.
    /// </summary>
    public void ToggleWindow()
    {
        if (windowPanel == null) return;

        windowPanel.SetActive(!windowPanel.activeSelf);

        if (windowPanel.activeSelf)
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// Reconstructs the contract list by clearing the current display and instantiating 
    /// new entries for each contract from the manager.
    /// </summary>
    public void RefreshUI()
    {
        if (contentArea == null || contractPrefab == null || ContractsManager.Instance == null) return;

        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        foreach (Contract c in ContractsManager.Instance.allContracts)
        {
            if (c.status == ContractStatus.Completed || c.status == ContractStatus.Failed)
                continue;

            GameObject newItem = Instantiate(contractPrefab, contentArea);
            TextMeshProUGUI[] texts = newItem.GetComponentsInChildren<TextMeshProUGUI>();
            Button actionButton = newItem.GetComponentInChildren<Button>();
            TextMeshProUGUI buttonText = actionButton != null ? actionButton.GetComponentInChildren<TextMeshProUGUI>() : null;

            if (texts.Length >= 2)
            {
                string tierColor = GetTierColorHex(c.tier);
                texts[0].text = $"<b>{c.contractType}</b> <color={tierColor}>[{c.tier}]</color>";

                ContractConfig config = ContractDatabase.GetConfig(c.contractType);
                string cycleTypeStr = c.cycleType.ToString();

                if (c.status == ContractStatus.Available)
                {
                    // Formats values for readability
                    texts[1].text = $"Type: {config.GetFormattedDescription()}\nQuota: {c.targetMWhPerCycle:F1} MWh / {cycleTypeStr} | Pay: <color=#00FF00>{c.rewardPerMWh:F0} €/MWh</color>\nDuration: {c.durationDays} days";
                }
                else if (c.status == ContractStatus.Active)
                {
                    texts[1].text = $"Cycle Progress: {c.deliveredInCurrentCycle:F1} / {c.targetMWhPerCycle:F1} MWh\nHealth: {c.satisfaction:F0}% | Time Left: {c.daysRemaining} days";
                }
            }

            if (actionButton != null && buttonText != null)
            {
                if (c.status == ContractStatus.Available)
                {
                    actionButton.interactable = true;
                    buttonText.text = "ACCEPT";
                    buttonText.color = Color.white;

                    int currentId = c.id;
                    actionButton.onClick.AddListener(() => OnAcceptButtonClicked(currentId));
                }
                else if (c.status == ContractStatus.Active)
                {
                    actionButton.interactable = false;
                    buttonText.text = "ACTIVE";
                    buttonText.color = Color.yellow;
                }
            }
        }
    }

    /// <summary>
    /// Handles the contract acceptance event.
    /// </summary>
    /// <param name="contractId">The unique ID of the contract.</param>
    private void OnAcceptButtonClicked(int contractId)
    {
        if (ContractsManager.Instance != null)
        {
            ContractsManager.Instance.AcceptContract(contractId);
            RefreshUI();
        }
    }

    /// <summary>
    /// Maps contract tiers to specific color codes.
    /// </summary>
    /// <param name="tier">The tier name string.</param>
    /// <returns>A hex color string.</returns>
    private string GetTierColorHex(string tier)
    {
        switch (tier)
        {
            case "Standard": return "#00FFFF";
            case "Urgent": return "#FF4444";
            case "VIP": return "#FF00FF";
            default: return "#FFFFFF";
        }
    }

    public bool IsWindowOpen()
    {
        return windowPanel != null && windowPanel.activeSelf;
    }
}