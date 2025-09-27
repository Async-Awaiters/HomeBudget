using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using AccountManagement.EF.Repositories.Interfaces;
using AccountManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly int millisecondsDelay;

    public AccountService(IAccountRepository accountRepository, IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        millisecondsDelay = int.Parse(configuration["OperationDelay"] ?? "1000");
    }

    public async Task<Account> GetAsync(Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        Account account = await _accountRepository.GetByIdAsync(userId, tokenSource.Token)
            ?? throw new EntityNotFoundException("Счет не найден.");

        return account;
    }

    public async Task<List<Account>> GetAllAsync(Guid accountId)
    {
        var accounts = await _accountRepository.GetAllAsync(accountId).ToListAsync();

        if (accounts == null || !accounts.Any())
            throw new EntityNotFoundException("Счета не найдены.");

        return accounts;
    }

    public async Task CreateAsync(Account account, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        if (await _accountRepository.GetByIdAsync(userId, tokenSource.Token) is not null)
            throw new EntityAlreadyExistsException("Счет уже существует.");

        await _accountRepository.CreateAsync(account, userId, tokenSource.Token);
    }

    public async Task UpdateAsync(Account account, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        if (await _accountRepository.GetByIdAsync(userId, tokenSource.Token) is null)
            throw new EntityNotFoundException("Счет не найден.");

        await _accountRepository.UpdateAsync(account, tokenSource.Token);
    }

    public async Task DeleteAsync(Guid accountId, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        if (await _accountRepository.GetByIdAsync(accountId, tokenSource.Token) is null)
            throw new EntityNotFoundException("Счет не найден.");

        await _accountRepository.DeleteAsync(accountId, userId, tokenSource.Token);
    }
}
