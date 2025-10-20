using AccountManagement.EF.Models;
using AccountManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

/// <summary>
/// Контроллер для управления банковскими счетами пользователей
/// </summary>
/// <remarks>
/// Реализует CRUD-операции над счетами через сервис IAccountService
/// Все методы требуют авторизации пользователя
/// </remarks>
[ApiController]
[Authorize]
public class AccountsController : AccountManagementBaseController
{
    private readonly IAccountService _accountService;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера
    /// </summary>
    /// <param name="logger">Логгер для регистрации событий</param>
    /// <param name="accountService">Сервис для работы со счетами</param>
    public AccountsController(ILogger<AccountsController> logger, IAccountService accountService)
        : base(logger)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Получает все активные счета текущего авторизованного пользователя
    /// </summary>
    /// <returns>Список счетов в формате JSON или статусный код</returns>
    /// <remarks>
    /// Извлекает ID пользователя из токена аутентификации
    /// Фильтрует только активные счета
    /// </remarks>
    [HttpGet]
    [Route("accounts")]
    [EndpointSummary("GetAllUserAccounts")]
    [EndpointDescription("Получение всех активных счетов пользователя")]
    public async Task<IActionResult> GetAllUserAccountsAsync()
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение ID пользователя из токена (предполагается, что middleware установила ClaimsPrincipal)
                var userId = GetUserId(HttpContext);
                var accounts = await _accountService.GetAllAsync(userId);
                return Ok(accounts);
            });
    }

    /// <summary>
    /// Получает конкретный счет по указанному идентификатору
    /// </summary>
    /// <param name="accountId">Уникальный идентификатор счета в формате GUID</param>
    /// <returns>Данные счета или статус 404 Not Found</returns>
    [HttpGet]
    [Route("accounts/{accountId:guid}")]
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

    /// <summary>
    /// Создает новый банковский счет для текущего пользователя
    /// </summary>
    /// <param name="account">Модель счета с данными для создания</param>
    /// <returns>Созданный счет с HTTP 201 Created или статус 403 Forbidden</returns>
    /// <exception cref="ForbiddenAccessException">Если UserId в модели не совпадает с текущим пользователем</exception>
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

    /// <summary>
    /// Обновляет данные существующего банковского счета
    /// </summary>
    /// <param name="accountId">Идентификатор счета для обновления</param>
    /// <param name="updatedAccount">Объект с новыми данными счета</param>
    /// <returns>Статус 200 OK при успешном обновлении или 403 Forbidden</returns>
    /// <exception cref="NotFoundException">Если указанный счет не существует</exception>
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

    /// <summary>
    /// Удаляет банковский счет по указанному идентификатору
    /// </summary>
    /// <param name="accountId">GUID счета для удаления</param>
    /// <returns>Статус 204 No Content при успешном удалении или 403 Forbidden</returns>
    /// <exception cref="NotFoundException">Если указанный счет не существует</exception>
    [HttpDelete]
    [Route("accounts/{accountId:guid}")]
    [EndpointSummary("DeleteAccount")]
    [EndpointDescription("Удаление счета")]
    public async Task<IActionResult> DeleteAccountAsync([FromRoute] Guid accountId)
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
