using HomeBudget.Directories.Data.Models;

namespace HomeBudget.Directories.Data.Interfaces;

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetAllAsync(CancellationToken ct);
    Task<Currency?> GetByIdAsync(Guid id, CancellationToken ct);
}