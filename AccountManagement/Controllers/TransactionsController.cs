using AccountManagement.EF.Models;
using AccountManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

/// <summary>
/// Контроллер для работы с транзакциями пользовательских счетов.
/// </summary>
[ApiController]
[Authorize]
public class TransactionsController : AccountManagementBaseController
{
    private readonly IAccountService _accountService;
    private readonly ITransactionsService _transactionsService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TransactionsController"/>.
    /// </summary>
    /// <param name="logger">Логгер для записи событий.</param>
    /// <param name="accountService">Сервис для работы со счетами.</param>
    /// <param name="transactionsService">Сервис для работы с транзакциями.</param>
    public TransactionsController(ILogger<TransactionsController> logger, IAccountService accountService, ITransactionsService transactionsService)
        : base(logger)
    {
        _accountService = accountService;
        _transactionsService = transactionsService;
    }

    /// <summary>
    /// Получает все транзакции, связанные с указанным аккаунтом.
    /// </summary>
    /// <param name="accountId">Идентификатор аккаунта.</param>
    /// <returns>Список транзакций или код ошибки.</returns>
    [HttpGet]
    [Route("transactions/{accountId:guid}")]
    public async Task<IActionResult> GetAccountTransactionsAsync([FromQuery] Guid accountId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение аккаунта и проверка владельца
                var account = await _accountService.GetAsync(accountId);
                if (account == null)
                    return NotFound("Аккаунт не найден");

                var userId = GetUserId(HttpContext);

                if (userId != account.UserId)
                    return Forbid("Доступ запрещён");

                // Получение транзакций
                var transactions = await _transactionsService.GetAllAsync(accountId);

                return Ok(transactions);
            });
    }

    /// <summary>
    /// Получает конкретную транзакцию по её идентификатору.
    /// </summary>
    /// <param name="transactionId">Идентификатор транзакции.</param>
    /// <returns>Объект транзакции или код ошибки.</returns>
    [HttpGet]
    [Route("transactions/{accountId:guid}/{transactionId:guid}")]
    public async Task<IActionResult> GetAccountTransactionAsync([FromRoute] Guid transactionId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение транзакции
                var transaction = await _transactionsService.GetAsync(transactionId);

                // Проверка владельца через связанный аккаунт
                var account = await _accountService.GetAsync(transaction.AccountId!.Value);
                var userId = GetUserId(HttpContext);
                if (account == null || userId != account.UserId)
                    return Forbid("Доступ запрещён");

                return Ok(transaction);
            });
    }

    /// <summary>
    /// Создает новую транзакцию.
    /// </summary>
    /// <param name="transaction">Объект транзакции для создания.</param>
    /// <returns>Созданный объект транзакции или код ошибки.</returns>
    [HttpPost]
    [Route("transactions")]
    public async Task<IActionResult> CreateTransactionAsync([FromBody] Transaction transaction)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                if (transaction is null)
                    throw new ArgumentNullException(nameof(transaction));

                var userId = GetUserId(HttpContext);

                // Создание транзакции
                await _transactionsService.CreateAsync(transaction, userId);
                return CreatedAtAction(nameof(CreateTransactionAsync), new { transactionId = transaction.Id }, transaction);
            });
    }

    /// <summary>
    /// Обновляет существующую транзакцию.
    /// </summary>
    /// <param name="updatedTransaction">Обновленный объект транзакции.</param>
    /// <returns>Статус успешного обновления или код ошибки.</returns>
    [HttpPut]
    [Route("transactions/{transactionId:guid}")]
    public async Task<IActionResult> UpdateTransactionAsync([FromBody] Transaction updatedTransaction)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                if (updatedTransaction is null)
                    throw new ArgumentNullException(nameof(updatedTransaction));

                var userId = GetUserId(HttpContext);

                // Обновление транзакции
                await _transactionsService.UpdateAsync(updatedTransaction, userId);
                return Ok();
            });
    }

    /// <summary>
    /// Удаляет указанную транзакцию.
    /// </summary>
    /// <param name="transactionId">Идентификатор транзакции для удаления.</param>
    /// <returns>Статус успешного удаления или код ошибки.</returns>
    [HttpDelete]
    [Route("transactions/{transactionId:guid}")]
    public async Task<IActionResult> DeleteTransactionAsync([FromRoute] Guid transactionId)
    {
        return await ExecuteWithLogging(
           async () =>
           {
               var userId = GetUserId(HttpContext);

               // Удаление транзакции
               await _transactionsService.DeleteAsync(transactionId, userId);
               return NoContent();
           });
    }

    /// <summary>
    /// Подтверждает указанную транзакцию.
    /// </summary>
    /// <param name="transactionId">Идентификатор транзакции для подтверждения.</param>
    /// <returns>Статус успешного подтверждения или код ошибки.</returns>
    [HttpPatch]
    [Route("transactions/{transactionId:guid}/confirm")]
    public async Task<IActionResult> ConfirmTransactionAsync([FromRoute] Guid transactionId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                var userId = GetUserId(HttpContext);

                // Подтверждение транзакции
                await _transactionsService.ConfirmAsync(transactionId, userId);
                return NoContent();
            });
    }
}
