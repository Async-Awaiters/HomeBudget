using HomeBudget.Directories.Services.Interfaces;
using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.Extensions.Options;

namespace HomeBudget.Directories.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoriesRepository _repository;
    private readonly ILogger<CategoryService> _logger;
    private readonly TimeSpan _defaultTimeout;

    public CategoryService(
            ICategoriesRepository repository,
            ILogger<CategoryService> logger,
            IOptions<ServiceTimeoutsOptions> options)
    {
        _repository = repository;
        _logger = logger;
        _defaultTimeout = TimeSpan.FromMilliseconds(options.Value.CategoryService);
    }

    public async Task<IEnumerable<Categories>> GetAllCategoriesAsync()
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Getting all categories");
        return await _repository.GetAll(cts.Token);
    }

    public async Task<Categories?> GetCategoryByIdAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogDebug("Getting category by ID: {CategoryId}", id);
        return await _repository.GetById(id, cts.Token);
    }

    public async Task<Categories> CreateCategoryAsync(Categories category)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Creating new category");
        await _repository.Create(category, cts.Token);
        return category;
    }

    public async Task<bool> UpdateCategoryAsync(Categories category)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Updating category {CategoryId}", category.Id);
        var updated = await _repository.Update(category, cts.Token);
        return updated;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Deleting category {CategoryId}", id);
        await _repository.Delete(id, cts.Token);
    }
}