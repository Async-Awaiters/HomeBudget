using HomeBudget.Directories.Data.Interfaces;
using HomeBudget.Directories.Data.Models;
using HomeBudget.Directories.Services.Interfaces;

namespace HomeBudget.Directories.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        ICategoryRepository repository,
        ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken ct)
    {
        _logger.LogInformation("Getting all categories");
        return await _repository.GetAllAsync(ct);
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken ct)
    {
        _logger.LogDebug("Getting category by ID: {CategoryId}", id);
        return await _repository.GetByIdAsync(id, ct);
    }

    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken ct)
    {
        _logger.LogInformation("Creating new category");
        category = category with { Id = Guid.NewGuid() }; //временно
        await _repository.AddAsync(category, ct);
        return category;
    }

    public async Task UpdateCategoryAsync(Category category, CancellationToken ct)
    {
        _logger.LogInformation("Updating category {CategoryId}", category.Id);
        await _repository.UpdateAsync(category, ct);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct)
    {
        _logger.LogInformation("Deleting category {CategoryId}", id);
        await _repository.DeleteAsync(id, ct);
    }
}