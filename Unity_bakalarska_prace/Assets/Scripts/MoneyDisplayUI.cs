using UnityEngine;
using TMPro;

public class MoneyDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged += UpdateMoneyText;

            // První update ruènì
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

    private void UpdateMoneyText(float currentBalance)
    {
        // "N0" formátuje èíslo s mezerami (napø. 1 000 000)
        moneyText.text = $"{currentBalance:N0} €";

        // Èervená barva, pokud jsme v dluhu (pokud to hra dovolí)
        if (currentBalance < 0) moneyText.color = Color.red;
        else moneyText.color = Color.white;
    }
}