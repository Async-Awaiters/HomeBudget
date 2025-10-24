using AccountManagement.EF.Models;
using AccountManagement.Exceptions;

namespace AccountManagement.TransactionProcessing.Strategies;

/// <summary>
/// Стратегия проверки транзакций для кредитных карт.
/// Выполняет проверку достаточности средств с учетом кредитного лимита.
/// </summary>
public class CreditCardCheckStrategy : ITransactionCheckStrategy
{
    /// <summary>
    /// Тип аккаунта, для которого применяется данная стратегия.
    /// </summary>
    public AccountTypes ForType => AccountTypes.CreditCard;

    /// <summary>
    /// Обрабатывает транзакцию по кредитной карте.
    /// Проверяет, не будет ли новое значение баланса превышать кредитный лимит.
    /// </summary>
    /// <param name="amount">Сумма транзакции.</param>
    /// <param name="account">Аккаунт, связанный с транзакцией.</param>
    /// <exception cref="InvalidTransactionException">
    /// Выбрасывается, если остаток средств на аккаунте меньше необходимой суммы
    /// с учетом кредитного лимита.
    /// </exception>
    public void ProcessTransaction(decimal amount, Account account)
    {
        var newBalance = account.Balance + amount;
        if (newBalance < 0)
        {
            // Если недостаточно средств и сумма превышает кредитный лимит
            if (-newBalance < account.CreditLimit)
            {
                throw new InvalidTransactionException("Not enough money");
            }
        }
    }
}
