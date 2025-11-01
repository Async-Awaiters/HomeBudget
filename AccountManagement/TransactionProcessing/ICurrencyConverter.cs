namespace AccountManagement.TransactionProcessing;

public interface ICurrencyConverter
{
    Task<decimal> ConvertToRublesAsync(decimal amount, Guid currencyId, string token);
    Task UpdateCurrencyRates(string token);
}
