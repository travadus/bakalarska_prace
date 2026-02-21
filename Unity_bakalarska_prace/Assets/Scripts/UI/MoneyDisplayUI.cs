using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI that shows money balance.
/// </summary>
public class MoneyDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged += UpdateMoneyText;

            UpdateMoneyText(EconomyManager.Instance.GetBalance());
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged -= UpdateMoneyText;
        }
    }

    /// <summary>
    /// Updates the money text.
    /// </summary>
    /// <param name="currentBalance">The new balance value.</param>
    private void UpdateMoneyText(float currentBalance)
    {
        moneyText.text = $"{currentBalance:N0} €";

        if (currentBalance < 0)
            moneyText.color = Color.red;
        else
            moneyText.color = Color.white;
    }
}