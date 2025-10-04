using AccountManagement.EF.Exceptions;
using AccountManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class BalanceController : AccountManagementBaseController
{
    private readonly IAccountService _accountService;

    public BalanceController(ILogger<AccountsController> logger, IAccountService accountService)
        : base(logger)
    {
        _accountService = accountService;
    }

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
