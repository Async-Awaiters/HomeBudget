using HomeBudget.Directories.Data.Interfaces;
using HomeBudget.Directories.Data.Models;
using HomeBudget.Directories.Services.Interfaces;

namespace HomeBudget.Directories.Services.Implementations;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repository;
    private readonly ILogger<CurrencyService> _logger;

    public CurrencyService(
        ICurrencyRepository repository,
        ILogger<CurrencyService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync(CancellationToken ct)
    {
        _logger.LogInformation("Getting all currencies");
        return await _repository.GetAllAsync(ct);
    }

    public async Task<Currency?> GetCurrencyByIdAsync(Guid id, CancellationToken ct)
    {
        _logger.LogDebug("Getting currency by ID: {CurrencyId}", id);
        return await _repository.GetByIdAsync(id, ct);
    }
}