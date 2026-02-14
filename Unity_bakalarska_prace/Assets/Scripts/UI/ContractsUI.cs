using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public void ToggleWindow()
    {
        if (windowPanel == null) return;

        windowPanel.SetActive(!windowPanel.activeSelf);

        if (windowPanel.activeSelf)
        {
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (contentArea == null || contractPrefab == null || ContractsManager.Instance == null) return;

        // 1. Clear old items
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // 2. Spawn current contracts
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
                // Title and Tier
                string tierColor = GetTierColorHex(c.tier);
                texts[0].text = $"<b>{c.contractType}</b> <color={tierColor}>[{c.tier}]</color>";

                // Details based on the new Data-Driven design
                ContractConfig config = ContractDatabase.GetConfig(c.contractType);
                string cycleTypeStr = c.cycleType.ToString();

                if (c.status == ContractStatus.Available)
                {
                    // :F1 oøízne kvótu na 1 des. místo, :F0 oøízne peníze na celá èísla
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

    private void OnAcceptButtonClicked(int contractId)
    {
        if (ContractsManager.Instance != null)
        {
            ContractsManager.Instance.AcceptContract(contractId);
            RefreshUI();
        }
    }

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