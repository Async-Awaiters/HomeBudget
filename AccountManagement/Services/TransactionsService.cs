using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using AccountManagement.EF.Repositories.Interfaces;
using AccountManagement.Exceptions;
using AccountManagement.Services.Interfaces;
using AccountManagement.TransactionProcessing;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Services;

/// <summary>
/// Сервис для управления транзакциями пользовательских счетов.
/// </summary>
/// <remarks>
/// Зависит от репозиториев транзакций и счетов, конфигурации и процессора транзакций.
/// </remarks>
public class TransactionsService : ITransactionsService
{
    /// <summary>
    /// Задержка операций в миллисекундах, взятая из конфигурации.
    /// </summary>
    private readonly int millisecondsDelay;

    /// <summary>
    /// Репозиторий для работы с транзакциями.
    /// </summary>
    private readonly ITransactionsRepository _transactionsRepository;

    /// <summary>
    /// Репозиторий для работы с пользовательскими счетами.
    /// </summary>
    private readonly IAccountRepository _accountRepository;

    /// <summary>
    /// Процессор для выполнения бизнес-логики транзакций.
    /// </summary>
    private readonly ITransactionProcessor _transactionProcessor;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TransactionsService"/>.
    /// </summary>
    /// <param name="transactionsRepository">Репозиторий транзакций.</param>
    /// <param name="accountRepository">Репозиторий счетов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="transactionProcessor">Процессор транзакций.</param>
    public TransactionsService(ITransactionsRepository transactionsRepository, IAccountRepository accountRepository,
                                IConfiguration configuration, ITransactionProcessor transactionProcessor)
    {
        _transactionsRepository = transactionsRepository;
        _accountRepository = accountRepository;
        _transactionProcessor = transactionProcessor;
        millisecondsDelay = int.Parse(configuration["OperationDelay"] ?? "1000");
    }

    /// <summary>
    /// Получает транзакцию по её идентификатору.
    /// </summary>
    /// <param name="transactionId">Идентификатор транзакции.</param>
    /// <returns>Объект транзакции.</returns>
    /// <exception cref="EntityNotFoundException">Если транзакция не найдена.</exception>
    public async Task<Transaction> GetAsync(Guid transactionId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        Transaction transaction = await _transactionsRepository.GetByIdAsync(transactionId, tokenSource.Token)
            ?? throw new EntityNotFoundException("Транзакция не найдена.");

        return transaction;
    }

    /// <summary>
    /// Получает список всех транзакций для указанного счета.
    /// </summary>
    /// <param name="accountId">Идентификатор счета.</param>
    /// <returns>Список транзакций.</returns>
    /// <exception cref="EntityNotFoundException">Если транзакции не найдены.</exception>
    public async Task<List<Transaction>> GetAllAsync(Guid accountId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        List<Transaction> transactions = await _transactionsRepository.GetAllAsync(accountId).ToListAsync();

        if (transactions == null || !transactions.Any())
            throw new EntityNotFoundException("Транзакции не найдены.");

        return transactions;
    }

    /// <summary>
    /// Создает новую транзакцию для указанного счета.
    /// </summary>
    /// <param name="transaction">Объект транзакции для создания.</param>
    /// <param name="userId">Идентификатор пользователя, инициирующего операцию.</param>
    /// <exception cref="EntityNotFoundException">Если счет не найден.</exception>
    /// <exception cref="AccessDeniedException">Если пользователь не имеет прав на счет.</exception>
    public async Task CreateAsync(Transaction transaction, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        using var accountTokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId!.Value, accountTokenSource.Token);
        if (account is null)
            throw new EntityNotFoundException("Счет не найден.");

        // Проверка прав пользователя
        if (account.UserId != userId)
            throw new AccessDeniedException("Недостаточно прав.");

        await _transactionProcessor.AddTransaction(transaction, account);

        // Обновление баланса счета
        account.Balance += transaction.Amount;
        using var updateTokenSource = new CancellationTokenSource(millisecondsDelay);
        await _accountRepository.UpdateAsync(account, updateTokenSource.Token);
    }

    /// <summary>
    /// Обновляет существующую транзакцию.
    /// </summary>
    /// <param name="transaction">Объект транзакции с обновленными данными.</param>
    /// <param name="userId">Идентификатор пользователя, инициирующего операцию.</param>
    /// <exception cref="EntityNotFoundException">Если транзакция или счет не найдены.</exception>
    /// <exception cref="AccessDeniedException">Если пользователь не имеет прав на счет.</exception>
    public async Task UpdateAsync(Transaction transaction, Guid userId)
    {
        // Проверка существования транзакции
        var existedTransaction = await GetAsync(transaction.Id);
        if (existedTransaction == null)
            throw new EntityNotFoundException("Транзакция не найдена");

        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        using var accountTokenSource = new CancellationTokenSource(millisecondsDelay);
        var account = await _accountRepository.GetByIdAsync(existedTransaction.AccountId!.Value, accountTokenSource.Token);
        if (account is null || userId != account.UserId)
            throw new AccessDeniedException("Доступ запрещён");

        await _transactionProcessor.UpdateTransaction(transaction, account);

        // Обновление баланса счета
        var diff = existedTransaction!.Amount - transaction.Amount;
        account.Balance += diff;
        using var updateTokenSource = new CancellationTokenSource(millisecondsDelay);
        await _accountRepository.UpdateAsync(account, updateTokenSource.Token);
    }

    /// <summary>
    /// Удаляет транзакцию.
    /// </summary>
    /// <param name="transactionId">Идентификатор транзакции.</param>
    /// <param name="userId">Идентификатор пользователя, инициирующего операцию.</param>
    /// <exception cref="EntityNotFoundException">Если транзакция или счет не найдены.</exception>
    /// <exception cref="AccessDeniedException">Если пользователь не имеет прав на счет.</exception>
    public async Task DeleteAsync(Guid transactionId, Guid userId)
    {
        // Проверка существования транзакции
        var transaction = await GetAsync(transactionId);
        if (transaction == null)
            throw new EntityNotFoundException("Транзакция не найдена");

        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        using var accountTokenSource = new CancellationTokenSource(millisecondsDelay);
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId!.Value, accountTokenSource.Token);
        if (account is null)
            throw new EntityNotFoundException("Счет не найден.");

        if (account.UserId != userId)
            throw new AccessDeniedException("Недостаточно прав.");

        await _transactionProcessor.RemoveTransaction(transaction, userId, account);

        // Обновление баланса счета
        account.Balance -= transaction.Amount;
        using var updateTokenSource = new CancellationTokenSource(millisecondsDelay);
        await _accountRepository.UpdateAsync(account, updateTokenSource.Token);
    }

    /// <summary>
    /// Подтверждает транзакцию (в разработке).
    /// </summary>
    /// <param name="transactionId">Идентификатор транзакции.</param>
    /// <param name="userId">Идентификатор пользователя, инициирующего операцию.</param>
    /// <exception cref="NotImplementedException">Метод ещё не реализован.</exception>
    public Task ConfirmAsync(Guid transactionId, Guid userId)
    {
        throw new NotImplementedException();
    }
}

