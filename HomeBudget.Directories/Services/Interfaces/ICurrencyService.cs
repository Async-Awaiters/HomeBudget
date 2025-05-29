using HomeBudget.Directories.Data.Models;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
    Task<Currency?> GetCurrencyByIdAsync(Guid id);
}