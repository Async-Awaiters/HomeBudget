using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using AccountManagement.EF.Repositories.Interfaces;
using AccountManagement.Exceptions;
using AccountManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Services;

public class TransactionsService : ITransactionsService
{
    private readonly int millisecondsDelay;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountRepository _accountRepository;

    public TransactionsService(ITransactionsRepository transactionsRepository, IAccountRepository accountRepository, IConfiguration configuration)
    {
        _transactionsRepository = transactionsRepository;
        _accountRepository = accountRepository;
        millisecondsDelay = int.Parse(configuration["OperationDelay"] ?? "1000");
    }

    public async Task<Transaction> GetAsync(Guid transactionId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        Transaction transaction = await _transactionsRepository.GetByIdAsync(transactionId, tokenSource.Token)
            ?? throw new EntityNotFoundException("Транзакция не найдена.");

        return transaction;
    }

    public async Task<List<Transaction>> GetAllAsync(Guid accountId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        List<Transaction> transactions = await _transactionsRepository.GetAllAsync(accountId).ToListAsync();

        if (transactions == null || !transactions.Any())
            throw new EntityNotFoundException("Транзакции не найдены.");

        return transactions;
    }

    public async Task CreateAsync(Transaction transaction, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        using var accountTokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId, accountTokenSource.Token);
        if (account is null)
            throw new EntityNotFoundException("Счет не найден.");

        // Проверка прав пользователя
        if (account.UserId != userId)
            throw new AccessDeniedException("Недостаточно прав.");

        await _transactionsRepository.CreateAsync(transaction, userId, tokenSource.Token);
    }

    public async Task UpdateAsync(Transaction transaction, Guid userId)
    {
        // Проверка существования транзакции
        var existedTransaction = await GetAsync(transaction.Id);
        if (existedTransaction == null)
            throw new EntityNotFoundException("Транзакция не найдена");

        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        using var accountTokenSource = new CancellationTokenSource(millisecondsDelay);
        var account = await _accountRepository.GetByIdAsync(existedTransaction.AccountId, accountTokenSource.Token);
        if (account == null || userId != account.UserId)
            throw new AccessDeniedException("Доступ запрещён");

        await _transactionsRepository.UpdateAsync(transaction, tokenSource.Token);
    }

    public async Task DeleteAsync(Guid transactionId, Guid userId)
    {
        // Проверка существования транзакции
        var transaction = await GetAsync(transactionId);
        if (transaction == null)
            throw new EntityNotFoundException("Транзакция не найдена");

        using var tokenSource = new CancellationTokenSource(millisecondsDelay);
        using var accountTokenSource = new CancellationTokenSource(millisecondsDelay);
        var account = await _accountRepository.GetByIdAsync(transaction.AccountId, accountTokenSource.Token);
        if (account is null)
            throw new EntityNotFoundException("Счет не найден.");

        if (account.UserId != userId)
            throw new AccessDeniedException("Недостаточно прав.");

        await _transactionsRepository.DeleteAsync(transaction.Id, userId, tokenSource.Token);
    }
}
