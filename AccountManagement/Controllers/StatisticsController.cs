using AccountManagement.EF.Models;
using AccountManagement.Models;
using AccountManagement.ReportBuilder;
using AccountManagement.Services.Interfaces;
using AccountManagement.TransactionProcessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountManagement.Controllers
{
    [ApiController]
    [Authorize]
    public class StatisticsController : AccountManagementBaseController
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionsService _transactionsService;
        private readonly ICurrencyConverter _currencyConverter;
        private IConfiguration _configuration;

        public StatisticsController(ILogger<StatisticsController> logger, IAccountService accountService,
                                    ITransactionsService transactionsService, IConfiguration configuration,
                                    ICurrencyConverter currencyConverter)
            : base(logger)
        {
            _accountService = accountService;
            _transactionsService = transactionsService;
            _configuration = configuration;
            _currencyConverter = currencyConverter;
        }

        /// <summary>
        /// Получает статистику за указанный период.
        /// </summary>
        /// <param name="from">Начало периода.</param>
        /// <param name="to">Конец периода.</param>
        /// <returns>Список транзакций или код ошибки.</returns>
        [HttpGet]
        [Route("statistics/{from}/{to}")]
        [EndpointSummary("GetPeriodStatistics")]
        [EndpointDescription("Получает статистику за указанный период.")]
        public async Task<IActionResult> GetAccountTransactionsAsync([FromRoute] DateTime from, [FromRoute] DateTime to)
        {
            return await ExecuteWithLogging(
                async () =>
                {
                    // Получение ID пользователя из токена
                    var userId = GetUserId(HttpContext);
                    // Получение счетов
                    var accounts = await _accountService.GetAllAsync(userId);
                    if (accounts == null || !accounts.Any())
                        return NotFound("Счета не найдены");

                    List<Transaction> userTransactions = new List<Transaction>();
                    // Получение транзакций для каждого аккаунта
                    foreach (var account in accounts)
                    {
                        var transactions = await _transactionsService.GetAllAsync(account.Id, from, to);
                        userTransactions.AddRange(transactions);
                    }

                    if (!userTransactions.Any())
                        return NotFound("Транзакции не найдены");

                    // Заполнение строк отчета
                    var reportDataRow = userTransactions
                        .Join(
                            accounts,
                            ut => ut.AccountId,
                            a => a.Id,
                            (t, a) => new ReportDataRow
                            {
                                CategoryId = t.CategoryId,
                                CurrencyId = a.CurrencyId,
                                Amount = t.Amount
                            });

                    // Формирование отчёта
                    string token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last() ?? "";
                    var statisticsReport = new StatisticsReportBuilder(reportDataRow,
                                                     _configuration["ExternalServicesURLs:CategoriesServices"] ?? string.Empty, _currencyConverter);

                    var report = await statisticsReport.Build(from, to, token);
                    return Ok(report);
                });
        }
    }
}
