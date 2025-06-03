using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeBudget.Directories.Services.Implementations;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyRepository _repository;
    private readonly ILogger<CurrencyService> _logger;
    private readonly TimeSpan _defaultTimeout;

    public CurrencyService(
        ICurrencyRepository repository,
        ILogger<CurrencyService> logger,
        IOptions<ServiceTimeoutsOptions> options)
    {
        _repository = repository;
        _logger = logger;
        _defaultTimeout = TimeSpan.FromMilliseconds(options.Value.CurrencyService);
    }

    public async Task<IEnumerable<Currency>> GetAllCurrenciesAsync()
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Getting all currencies");
        return await _repository.GetAll(cts.Token).ToListAsync();
    }

    public async Task<Currency?> GetCurrencyByIdAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogDebug("Getting currency by ID: {CurrencyId}", id);
        return await _repository.GetById(id, cts.Token);
    }
}