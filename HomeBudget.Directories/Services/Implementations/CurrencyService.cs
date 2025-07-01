using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Directories.Services.Implementations;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repository;
    private readonly ILogger<CurrencyService> _logger;
    private readonly TimeSpan _timeout;
    private const int _defaultTimeout = 30000;

    public CurrencyService(
        ICurrencyRepository repository,
        ILogger<CurrencyService> logger,
            IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;
        int timeoutMs = configuration.GetValue("Services:Timeouts:CurrencyService", _defaultTimeout);
        _timeout = TimeSpan.FromMilliseconds(timeoutMs);
    }

    public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Getting all currencies");
        return await _repository.GetAll(cts.Token).ToListAsync();
    }

    public async Task<Currency?> GetCurrencyByIdAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogDebug("Getting currency by ID: {CurrencyId}", id);
        return await _repository.GetById(id, cts.Token);
    }
}