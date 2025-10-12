using AccountManagement.EF.Exceptions;
using AccountManagement.Services.Interfaces;
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

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BalanceController"/>
    /// </summary>
    /// <param name="logger">Логгер для записи событий</param>
    /// <param name="accountService">Сервис для работы с аккаунтами</param>
    public BalanceController(ILogger<AccountsController> logger, IAccountService accountService)
        : base(logger)
    {
        _accountService = accountService;
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

                // Получение всех активных счетов пользователя
                var userAccounts = await _accountService.GetAllAsync(userId);

                // Проверка на null и наличие элементов
                if (userAccounts == null || !userAccounts.Any())
                    throw new EntityNotFoundException($"Аккаунты не найдены для пользователя с ID: {userId}");

                // Суммирование балансов всех активных счетов
                var userBalance = userAccounts
                    .Where(acc => acc.IsActive)
                    .Sum(acc => acc.Balance);

                return Ok(new { TotalBalance = userBalance });
            });
    }
}
