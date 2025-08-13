using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICurrencyService
{
    Task<IEnumerable<Currency>> GetAllCurrenciesAsync();
    Task<Currency?> GetCurrencyByIdAsync(Guid id);
}