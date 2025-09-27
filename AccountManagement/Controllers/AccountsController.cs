using AccountManagement.EF.Models;
using AccountManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

[ApiController]
public class AccountsController : AccountManagementBaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(ILogger<AccountsController> logger, IAccountService accountService)
        : base(logger)
    {
        _accountService = accountService;
    }

    [HttpGet]
    [Route("accounts/{userId:guid}")]
    [EndpointSummary("GetAllUserAccounts")]
    [EndpointDescription("Получение всех активных счетов пользователя")]
    public async Task<IActionResult> GetAllUserAccountsAsync([FromQuery] Guid userId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                var accounts = await _accountService.GetAllAsync(userId);
                return Ok(accounts);
            });
    }

    [HttpGet]
    [Route("account/{accountId:guid}")]
    [EndpointSummary("GetAccount")]
    [EndpointDescription("Получение конкретного счета по ID")]
    public async Task<IActionResult> GetAccountAsync([FromRoute] Guid accountId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                var account = await _accountService.GetAsync(accountId);
                return Ok(account);
            });
    }

    [HttpPost]
    [Route("accounts")]
    [EndpointSummary("CreateAccount")]
    [EndpointDescription("Создание нового счета")]
    public async Task<IActionResult> CreateAccountAsync([FromBody] Account account)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение ID пользователя из токена (предполагается, что middleware установила ClaimsPrincipal)
                var userId = GetUserId(HttpContext);

                if (userId != account.UserId)
                    return Forbid("Доступ запрещён");

                await _accountService.CreateAsync(account, userId);

                return CreatedAtAction(nameof(GetAccountAsync), new { accountId = account.Id }, account);
            });
    }

    [HttpPut]
    [Route("accounts/{accountId:guid}")]
    [EndpointSummary("UpdateAccount")]
    [EndpointDescription("Обновление данных счета")]
    public async Task<IActionResult> UpdateAccountAsync([FromRoute] Guid accountId, [FromBody] Account updatedAccount)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение оригинального счета
                var originalAccount = await _accountService.GetAsync(accountId);

                // Проверка владельца
                var userId = GetUserId(HttpContext);
                if (userId != originalAccount.UserId)
                    return Forbid("Доступ запрещён");

                // Обновление данных
                updatedAccount.Id = accountId;  // Убедимся, что ID совпадает
                await _accountService.UpdateAsync(updatedAccount, userId);

                return Ok();
            });
    }

    [HttpDelete]
    [Route("accounts/{accountId:guid}")]
    [EndpointSummary("DeleteAccount")]
    [EndpointDescription("Удаление счета")]
    public async Task<IActionResult> DeleteAccountAsync([FromRoute] Guid accountId, [FromQuery] Guid userId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение аккаунта перед удалением
                var account = await _accountService.GetAsync(accountId);

                // Проверка владельца
                var userId = GetUserId(HttpContext);
                if (userId != account.UserId)
                    return Forbid("Доступ запрещён");

                await _accountService.DeleteAsync(accountId, userId);
                return NoContent();
            });
    }
}
