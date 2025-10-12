using AccountManagement.EF.Models;
using AccountManagement.Exceptions;

namespace AccountManagement.TransactionProcessing.Strategies;

/// <summary>
/// Стратегия проверки для наличности.
/// Реализует логику проверки транзакций для счетов типа Cash.
/// </summary>
public class CashCheckStrategy : ITransactionCheckStrategy
{
    /// <summary>
    /// Тип аккаунта, для которого применяется данная стратегия.
    /// </summary>
    public AccountTypes ForType => AccountTypes.Cash;

    /// <summary>
    /// Выполняет проверку транзакции для счета типа Cash.
    /// </summary>
    /// <param name="amount">Сумма транзакции.</param>
    /// <param name="account">Аккаунт, к которому применяется транзакция.</param>
    /// <exception cref="InvalidTransactionException">
    /// Выбрасывается, если баланс аккаунта недостаточен для выполнения транзакции.
    /// </exception>
    public void ProcessTransaction(decimal amount, Account account)
    {
        if (account.Balance - amount < 0)
        {
            throw new InvalidTransactionException("Not enough money");
        }
    }
}
