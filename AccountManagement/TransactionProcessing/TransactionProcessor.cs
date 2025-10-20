using AccountManagement.EF.Models;
using AccountManagement.EF.Repositories.Interfaces;
using AccountManagement.Exceptions;

namespace AccountManagement.TransactionProcessing;

/// <summary>
/// Класс, реализующий логику обработки транзакций для учетных записей.
/// </summary>
public class TransactionProcessor : ITransactionProcessor
{
    /// <summary>
    /// Задержка в миллисекундах для операций с транзакциями.
    /// </summary>
    private const int millisecondsDelay = 5000;

    /// <summary>
    /// Репозиторий для работы с транзакциями.
    /// </summary>
    private readonly ITransactionsRepository _transactionsRepository;

    /// <summary>
    /// Фабрика стратегий обработки транзакций.
    /// </summary>
    private readonly ITransactionStrategyFactory _transactionStrategyFactory;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TransactionProcessor"/>.
    /// </summary>
    /// <param name="transactionsRepository">Репозиторий для работы с транзакциями.</param>
    /// <param name="transactionStrategyFactory">Фабрика стратегий обработки транзакций.</param>
    public TransactionProcessor(ITransactionsRepository transactionsRepository,
        ITransactionStrategyFactory transactionStrategyFactory)
    {
        _transactionsRepository = transactionsRepository;
        _transactionStrategyFactory = transactionStrategyFactory;
    }

    /// <summary>
    /// Добавляет новую транзакцию к указанному аккаунту.
    /// </summary>
    /// <param name="transaction">Транзакция для добавления.</param>
    /// <param name="account">Аккаунт, к которому применяется транзакция.</param>
    /// <exception cref="InvalidTransactionException">Если транзакция недействительна.</exception>
    public async Task AddTransaction(Transaction transaction, Account account)
    {
        var strategy = _transactionStrategyFactory.GetStrategy(account.Type);
        try
        {
            strategy.ProcessTransaction(transaction.Amount, account);
        }
        catch (InvalidTransactionException)
        {
            throw;
        }

        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        await _transactionsRepository.CreateAsync(transaction, tokenSource.Token);
    }

    /// <summary>
    /// Удаляет транзакцию из указанного аккаунта.
    /// </summary>
    /// <param name="transaction">Транзакция для удаления.</param>
    /// <param name="userId">Идентификатор пользователя, инициировавшего удаление.</param>
    /// <param name="account">Аккаунт, с которого удаляется транзакция.</param>
    /// <exception cref="InvalidTransactionException">Если транзакция недействительна.</exception>
    public async Task RemoveTransaction(Transaction transaction, Guid userId, Account account)
    {
        var strategy = _transactionStrategyFactory.GetStrategy(account.Type);
        try
        {
            strategy.ProcessTransaction(-transaction.Amount, account);
        }
        catch (InvalidTransactionException)
        {
            throw;
        }

        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        await _transactionsRepository.DeleteAsync(transaction.Id, userId, tokenSource.Token);
    }

    /// <summary>
    /// Обновляет существующую транзакцию для указанного аккаунта.
    /// </summary>
    /// <param name="transaction">Обновленная транзакция.</param>
    /// <param name="account">Аккаунт, к которому применяется обновление.</param>
    /// <exception cref="InvalidTransactionException">Если транзакция недействительна.</exception>
    public async Task UpdateTransaction(Transaction transaction, Account account)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        var oldTransaction = await _transactionsRepository.GetByIdAsync(transaction.Id, tokenSource.Token);

        if (oldTransaction?.Amount != transaction.Amount)
        {
            var strategy = _transactionStrategyFactory.GetStrategy(account.Type);

            try
            {
                var diff = oldTransaction!.Amount - transaction.Amount;
                strategy.ProcessTransaction(diff, account);
            }
            catch (InvalidTransactionException)
            {
                throw;
            }
        }

        await _transactionsRepository.UpdateAsync(transaction, tokenSource.Token);
    }
}

