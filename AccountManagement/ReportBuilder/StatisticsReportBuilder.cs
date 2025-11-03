using System.Text.Json;
using AccountManagement.Models;
using AccountManagement.TransactionProcessing;

namespace AccountManagement.ReportBuilder;

public class StatisticsReportBuilder : IReportBuilder
{
    private readonly IEnumerable<ReportDataRow> _dataRows;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly string _categoriesURL;
    private Dictionary<Guid, decimal> _categoryAmounts = new Dictionary<Guid, decimal>();

    public StatisticsReportBuilder(IEnumerable<ReportDataRow> dataRows, string categoriesURL, ICurrencyConverter currencyConverter)
    {
        _dataRows = dataRows;
        _currencyConverter = currencyConverter;
        _categoriesURL = categoriesURL;
    }

    /// <summary>
    /// Метод для построения отчета
    /// </summary>
    /// <param name="from"> Дата начала периода. </param>
    /// <param name="to"> Дата конца периода. </param>
    /// <param name="token"> Токен авторизации. </param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<StatisticsReport> Build(DateTime from, DateTime to, string token)
    {
        var report = new StatisticsReport
        {
            TotalAmount = 0m,
            From = DateOnly.FromDateTime(from),
            To = DateOnly.FromDateTime(to)
        };

        // Группировка по категории с поиском суммы для каждой категории
        foreach (var row in _dataRows)
        {
            var amount = await _currencyConverter.ConvertToRublesAsync(row.Amount, row.CurrencyId, token);
            if (!_categoryAmounts.ContainsKey(row.CategoryId))
                _categoryAmounts.Add(row.CategoryId, amount);
            else
                _categoryAmounts[row.CategoryId] += amount;
            report.TotalAmount += amount;
        }

        // Преобразование словаря в список для отчета
        HttpClient client = new HttpClient();
        foreach (var categoryAmount in _categoryAmounts)
        {
            var response = await client.GetAsync(string.Format(_categoriesURL, categoryAmount.Key));
            string categoryName;

            // Проверяем статус ответа
            if (response.IsSuccessStatusCode)
            {
                // Получаем содержимое ответа в виде строки
                var json = await response.Content.ReadAsStringAsync();

                // Десериализуем строку в объект
                categoryName = JsonSerializer.Deserialize<CategoryResponse>(json)?.Name ?? "Unknown";
            }
            else
            {
                throw new Exception("Failed to fetch currency rates");
            }

            report.CategoryReport.Add(new CategoryReportRow
            {
                CategoryId = categoryAmount.Key,
                Amount = categoryAmount.Value,
                CategoryName = categoryName
            });
        }

        return report;
    }
}
