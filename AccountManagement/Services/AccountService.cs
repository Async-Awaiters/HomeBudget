using AccountManagement.EF.Exceptions;
using AccountManagement.EF.Models;
using AccountManagement.EF.Repositories.Interfaces;
using AccountManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AccountManagement.Services;

/// <summary>
/// Сервис для управления банковскими счетами пользователей
/// </summary>
public class AccountService : IAccountService
{
    /// <summary>
    /// Репозиторий для работы с данными счетов
    /// </summary>
    private readonly IAccountRepository _accountRepository;

    /// <summary>
    /// Задержка операций в миллисекундах, взятая из конфигурации
    /// </summary>
    private readonly int millisecondsDelay;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="AccountService"/>
    /// </summary>
    /// <param name="accountRepository">Репозиторий для работы с данными</param>
    /// <param name="configuration">Конфигурационные параметры приложения</param>
    public AccountService(IAccountRepository accountRepository, IConfiguration configuration)
    {
        _accountRepository = accountRepository;
        millisecondsDelay = int.Parse(configuration["OperationDelay"] ?? "1000");
    }

    /// <summary>
    /// Получает счет пользователя по идентификатору
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>Объект <see cref="Account"/> с данными пользователя</returns>
    /// <exception cref="EntityNotFoundException">Если счет не найден</exception>
    public async Task<Account> GetAsync(Guid accountId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        Account account = await _accountRepository.GetByIdAsync(accountId, tokenSource.Token)
            ?? throw new EntityNotFoundException("Счет не найден.");

        return account;
    }

    /// <summary>
    /// Получает список всех счетов пользователя
    /// </summary>
    /// <param name="accountId">Идентификатор аккаунта</param>
    /// <returns>Список объектов <see cref="Account"/></returns>
    /// <exception cref="EntityNotFoundException">Если счета не найдены</exception>
    public async Task<List<Account>> GetAllAsync(Guid accountId, DateTime? from = null, DateTime? to = null)
    {
        var accounts = await _accountRepository.GetAllAsync(accountId).ToListAsync();

        if (accounts == null || !accounts.Any())
            throw new EntityNotFoundException("Счета не найдены.");

        return accounts;
    }

    /// <summary>
    /// Создает новый банковский счет
    /// </summary>
    /// <param name="account">Данные нового счета</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <exception cref="EntityAlreadyExistsException">Если счет с таким идентификатором уже существует</exception>
    public async Task CreateAsync(Account account, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        if (await _accountRepository.GetByIdAsync(userId, tokenSource.Token) is not null)
            throw new EntityAlreadyExistsException("Счет уже существует.");

        await _accountRepository.CreateAsync(account, tokenSource.Token);
    }

    /// <summary>
    /// Обновляет данные существующего счета
    /// </summary>
    /// <param name="account">Обновленные данные счета</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <exception cref="EntityNotFoundException">Если счет не найден</exception>
    public async Task UpdateAsync(Account account, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        var existed = await _accountRepository.GetByIdAsync(account.Id, tokenSource.Token);
        if (existed is null)
            throw new EntityNotFoundException("Счет не найден.");

        await _accountRepository.UpdateAsync(account, tokenSource.Token);
    }

    /// <summary>
    /// Удаляет банковский счет
    /// </summary>
    /// <param name="accountId">Идентификатор удаляемого счета</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <exception cref="EntityNotFoundException">Если счет не найден</exception>
    public async Task DeleteAsync(Guid accountId, Guid userId)
    {
        using var tokenSource = new CancellationTokenSource(millisecondsDelay);

        // Проверка существования счета
        if (await _accountRepository.GetByIdAsync(accountId, tokenSource.Token) is null)
            throw new EntityNotFoundException("Счет не найден.");

        await _accountRepository.DeleteAsync(accountId, userId, tokenSource.Token);
    }
}
