using System.Text.Json;
using AccountManagement.Models;

namespace AccountManagement.TransactionProcessing;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly string _currencyRatesURL;
    private readonly string _currencyIdsURL;

    private static Dictionary<Guid, decimal> CurrencyRates = new();
    private static Dictionary<string, Guid> CurrencyIds = new();

    private static DateOnly LastUpdate;

    public CurrencyConverter(string CurrencyRatesURL, string CurrencyIdsURL)
    {
        _currencyRatesURL = CurrencyRatesURL;
        _currencyIdsURL = CurrencyIdsURL;
    }

    public async Task<decimal> ConvertToRublesAsync(decimal amount, Guid currencyId, string token)
    {
        if (CurrencyRates.Count == 0 || LastUpdate != DateOnly.FromDateTime(DateTime.Now))
            await UpdateCurrencyRates(token);

        // В реальной реализации конвертации можно использовать сторонние сервисы или библиотеки
        return CurrencyRates[currencyId] * amount;
    }

    public async Task UpdateCurrencyRates(string token)
    {
        CurrencyRates.Clear();
        var newRates = await GetRates(token);
        foreach (var (currency, rate) in newRates)
        {
            CurrencyRates[currency] = rate;
        }
    }

    private async Task<Dictionary<Guid, decimal>> GetRates(string token)
    {
        if (CurrencyIds.Count == 0)
            CurrencyIds = await GetCurrencyIds(token);

        // Создаем экземпляр HttpClient
        var client = new HttpClient();

        // Отправляем запрос и получаем ответ
        var response = await client.GetAsync(_currencyRatesURL);
        CurrencyData jsonData;

        // Проверяем статус ответа
        if (response.IsSuccessStatusCode)
        {
            // Получаем содержимое ответа в виде строки
            var json = await response.Content.ReadAsStringAsync();

            // Десериализуем строку в объект
            jsonData = JsonSerializer.Deserialize<CurrencyData>(json)!;
        }
        else
        {
            throw new Exception("Failed to fetch currency rates");
        }

        jsonData.Valute.Add("RUB",
            new Valute { Value = 1, CharCode = "RUB", NumCode = "643", Name = "Российский рубль", ID = "R0001" });

        var newRates = new Dictionary<Guid, decimal>();

        foreach (var currencyId in CurrencyIds)
            newRates[currencyId.Value] = jsonData.Valute[currencyId.Key].Value;

        LastUpdate = DateOnly.FromDateTime(DateTime.Now);

        return newRates;
    }

    private async Task<Dictionary<string, Guid>> GetCurrencyIds(string token)
    {
        // Создаем экземпляр HttpClient
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Отправляем запрос и получаем ответ
        var response = await client.GetAsync(_currencyIdsURL);
        List<CurrencyIdsData> jsonData;

        // Проверяем статус ответа
        if (response.IsSuccessStatusCode)
        {
            // Получаем содержимое ответа в виде строки
            var json = await response.Content.ReadAsStringAsync();

            // Десериализуем строку в объект
            jsonData = JsonSerializer.Deserialize<List<CurrencyIdsData>>(json)!;
        }
        else
        {
            throw new Exception("Failed to fetch currency ids");
        }

        var currencyIds = new Dictionary<string, Guid>();
        foreach (var currency in jsonData)
            currencyIds.Add(currency.Code, currency.Id);
        return currencyIds;
    }
}
