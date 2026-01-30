using System;

[Serializable]
public struct MoneyTransaction
{
    public DateTime Date;
    public float Amount;      // Kladné = pøíjem, Záporné = výdaj
    public string Description; // Napø. "Prodej energie"
    public float BalanceAfter; // Kolik zbylo po transakci

    public MoneyTransaction(DateTime date, float amount, string description, float balanceAfter)
    {
        Date = date;
        Amount = amount;
        Description = description;
        BalanceAfter = balanceAfter;
    }
}