using System;

/// <summary>
/// Represents a single record of a financial transaction.
/// </summary>
[Serializable]
public struct MoneyTransaction
{
    public DateTime Date;
    public float Amount;
    public string Description;
    public float BalanceAfter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoneyTransaction"/> structure.
    /// </summary>
    /// <param name="date">Timestamp of the transaction.</param>
    /// <param name="amount">The financial change.</param>
    /// <param name="description">description for the transaction.</param>
    /// <param name="balanceAfter">The resulting account balance.</param>
    public MoneyTransaction(DateTime date, float amount, string description, float balanceAfter)
    {
        Date = date;
        Amount = amount;
        Description = description;
        BalanceAfter = balanceAfter;
    }
}