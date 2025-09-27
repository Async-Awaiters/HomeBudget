using AccountManagement.EF.Models;
using AccountManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers;

[ApiController]
public class TransactionsController : AccountManagementBaseController
{
    private readonly IAccountService _accountService;
    private readonly ITransactionsService _transactionsService;

    public TransactionsController(ILogger<TransactionsController> logger, IAccountService accountService, ITransactionsService transactionsService)
        : base(logger)
    {
        _accountService = accountService;
        _transactionsService = transactionsService;
    }

    [HttpGet]
    [Route("transactions/{accountId:guid}")]
    [EndpointSummary("GetAccountTransactions")]
    [EndpointDescription("Получение всех транзакций по ID аккаунта")]
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

    [HttpGet]
    [Route("transactions/{accountId:guid}/{transactionId:guid}")]
    [EndpointSummary("GetTransaction")]
    [EndpointDescription("Получение транзакции по ID")]
    public async Task<IActionResult> GetAccountTransactionAsync([FromRoute] Guid transactionId)
    {
        return await ExecuteWithLogging(
            async () =>
            {
                // Получение транзакции
                var transaction = await _transactionsService.GetAsync(transactionId);

                // Проверка владельца через связанный аккаунт
                var account = await _accountService.GetAsync(transaction.AccountId);
                var userId = GetUserId(HttpContext);
                if (account == null || userId != account.UserId)
                    return Forbid("Доступ запрещён");

                return Ok(transaction);
            });
    }

    [HttpPost]
    [Route("transactions")]
    [EndpointSummary("CreateTransaction")]
    [EndpointDescription("Создание новой транзакции")]
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

    [HttpPut]
    [Route("transactions/{transactionId:guid}")]
    [EndpointSummary("UpdateTransaction")]
    [EndpointDescription("Обновление транзакции")]
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

    [HttpDelete]
    [Route("transactions/{transactionId:guid}")]
    [EndpointSummary("DeleteTransaction")]
    [EndpointDescription("Удаление транзакции")]
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

    /*[HttpPatch]
    [Route("transactions/{transactionId:guid}/confirm")]
    [EndpointSummary("ConfirmTransaction")]
    [EndpointDescription("Подтверждение транзакции")]
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
    }*/
}
