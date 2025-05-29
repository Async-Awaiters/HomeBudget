using HomeBudget.Directories.Data.Interfaces;
using HomeBudget.Directories.Data.Models;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HomeBudget.Directories.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;
    private readonly TimeSpan _defaultTimeout;

    public CategoryService(
            ICategoryRepository repository,
            ILogger<CategoryService> logger,
            IOptions<ServiceTimeoutsOptions> options)
    {
        _repository = repository;
        _logger = logger;
        _defaultTimeout = TimeSpan.FromMilliseconds(options.Value.CategoryService);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Getting all categories");
        return await _repository.GetAllAsync(cts.Token);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogDebug("Getting category by ID: {CategoryId}", id);
        return await _repository.GetByIdAsync(id, cts.Token);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Creating new category");
        category = category with { Id = Guid.NewGuid() }; //временно
        await _repository.AddAsync(category, cts.Token);
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Updating category {CategoryId}", category.Id);
        await _repository.UpdateAsync(category, cts.Token);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_defaultTimeout);
        _logger.LogInformation("Deleting category {CategoryId}", id);
        await _repository.DeleteAsync(id, cts.Token);
    }
}