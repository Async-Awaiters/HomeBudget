using AccountManagement.EF.Exceptions;
using AccountManagement.Services.Interfaces;
using AccountManagement.TransactionProcessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

/// <summary>
/// Контроллер для управления балансом пользователей
/// </summary>
/// <remarks>
/// Реализует функциональность получения общего баланса пользователя
/// по всем активным счетам через HTTP API.
/// Требует авторизации для доступа.
/// </remarks>
[ApiController]
[Authorize]
[Route("[controller]")]
public class BalanceController : AccountManagementBaseController
{
    private readonly IAccountService _accountService;
    private readonly ICurrencyConverter _currencyConverter;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BalanceController"/>
    /// </summary>
    /// <param name="logger">Логгер для записи событий</param>
    /// <param name="accountService">Сервис для работы с аккаунтами</param>
    public BalanceController(ILogger<BalanceController> logger, IAccountService accountService, ICurrencyConverter currencyConverter)
        : base(logger)
    {
        _accountService = accountService;
        _currencyConverter = currencyConverter;
    }

    /// <summary>
    /// Возвращает баланс пользователя в рублях.
    /// </summary>
    private async Task<decimal> BalanceInRubles(Guid currencyId, decimal balance, string token)
    {
        return await _currencyConverter.ConvertToRublesAsync(balance, currencyId, token);
    }

    /// <summary>
    /// Асинхронно возвращает общий баланс пользователя
    /// </summary>
    /// <returns>HTTP-ответ с объектом содержащим поле TotalBalance</returns>
    /// <exception cref="EntityNotFoundException">Если у пользователя нет активных аккаунтов</exception>
    [HttpGet]
    [EndpointSummary("GetUserBalance")]
    [EndpointDescription("Получение баланса пользователя по всем активным счетам")]
    public async Task<IActionResult> GetUserBalanceAsync()
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение ID пользователя из токена
                var userId = GetUserId(HttpContext);
                decimal userBalance = 0;
                string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last() ?? "";
                await _currencyConverter.UpdateCurrencyRates(token);

                // Ожидание доступа к критической секции
                await _semaphore.WaitAsync();
                try
                {
                    // Получение всех активных счетов пользователя
                    var userAccounts = await _accountService.GetAllAsync(userId);

                    // Проверка на null и наличие элементов
                    if (userAccounts == null || !userAccounts.Any())
                        throw new EntityNotFoundException($"Аккаунты не найдены для пользователя с ID: {userId}");

                    // Фильтрация активных счётов
                    var activeAccounts = userAccounts.Where(acc => acc.IsActive).ToList();

                    // Объект для синхронизации (необходим, так как lock работает только со ссылками)
                    object balanceLock = new object();

                    // Параллельное суммирование с использованием локальных накопителей
                    await Parallel.ForEachAsync(activeAccounts, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 8 // ограничить количество одновременных вызовов
                    }, async (account, _) => // здесь _ — это CancellationToken, не используемый
                    {
                        decimal balance = await BalanceInRubles(account.CurrencyId, account.Balance, token);

                        lock (balanceLock)
                        {
                            userBalance += balance;
                        }
                    });
                }
                finally
                {
                    // Освобождение семафора
                    _semaphore.Release();
                }

                return Ok(new { TotalBalance = userBalance });
            });
    }
}
