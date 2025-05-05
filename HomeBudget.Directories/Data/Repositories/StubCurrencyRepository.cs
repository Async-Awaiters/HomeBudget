using HomeBudget.Directories.Data.Interfaces;
using HomeBudget.Directories.Data.Models;

namespace HomeBudget.Directories.Data.Repositories;

public class StubCurrencyRepository : ICurrencyRepository
{
    private readonly List<Currency> _currencies = new()
    {
        new Currency(Guid.Parse("44444444-4444-4444-4444-444444444444"), "USD", "US Dollar"),
        new Currency(Guid.Parse("55555555-5555-5555-5555-555555555555"), "EUR", "Euro"),
        new Currency(Guid.Parse("66666666-6666-6666-6666-666666666666"), "GBP", "British Pound")
    };

    public Task<IEnumerable<Currency>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<IEnumerable<Currency>>(_currencies);
    }

    public Task<Currency?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_currencies.FirstOrDefault(c => c.Id == id));
    }
}