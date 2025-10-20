using AccountManagement.EF.Models;
using AccountManagement.Exceptions;

namespace AccountManagement.TransactionProcessing.Strategies;

/// <summary>
/// Стратегия проверки дебетовой карты.
/// </summary>
/// <remarks>
/// Реализует интерфейс <see cref="ITransactionCheckStrategy"/> для проверки транзакций дебетовых карт.
/// Выполняет проверку достаточности средств с учетом лимита овердрафта.
/// </remarks>
public class DebitCardCheckStrategy : ITransactionCheckStrategy
{
    /// <summary>
    /// Тип аккаунта, для которого применяется данная стратегия.
    /// </summary>
    public AccountTypes ForType => AccountTypes.DebitCard;

    /// <summary>
    /// Выполняет проверку транзакции для дебетовой карты.
    /// </summary>
    /// <param name="amount">Сумма транзакции.</param>
    /// <param name="account">Аккаунт, связанный с транзакцией.</param>
    /// <exception cref="InvalidTransactionException">
    /// Выбрасывается, если сумма транзакции превышает доступный баланс и лимит овердрафта.
    /// </exception>
    /// <remarks>
    /// Логика проверки:
    /// 1. Рассчитывается новый баланс после транзакции.
    /// 2. Если баланс становится отрицательным, но его абсолютное значение меньше лимита овердрафта,
    ///    транзакция считается недопустимой.
    /// </remarks>
    public void ProcessTransaction(decimal amount, Account account)
    {
        var newBalance = account.Balance - amount;
        if (newBalance < 0)
        {
            if (-newBalance < account.OverdraftLimit)
            {
                throw new InvalidTransactionException("Not enough money");
            }
        }
    }
}
