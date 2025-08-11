using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeBudget.Directories.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoriesRepository _repository;
    private readonly ILogger<CategoryService> _logger;
    private readonly TimeSpan _timeout;
    private const int _defaultTimeout = 30000;

    public CategoryService(ICategoriesRepository repository, ILogger<CategoryService> logger, IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;
        int timeoutMs = configuration.GetValue("Services:Timeouts:CategoryService", _defaultTimeout);
        _timeout = TimeSpan.FromMilliseconds(timeoutMs);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync(Guid userId)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Getting all categories");
        return await _repository.GetAll(userId, cts.Token).ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid userId, Guid id)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogDebug("Getting category by ID: {CategoryId}", id);
        return await _repository.GetById(userId, id, cts.Token);
    }

    public async Task<Category> CreateCategoryAsync(Guid userId, Category category)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Creating new category");

        try
        {
            category.UserId = userId;
            await _repository.Create(category, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating category: {ErrorMessage}", ex.Message);
            throw;
        }

        return category;
    }

    public async Task UpdateCategoryAsync(Guid userId, Guid id, Category category)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Updating category {CategoryId}", category.Id);
        try
        {
            await _repository.Update(userId, id, category, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating category: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task DeleteCategoryAsync(Guid userId, Guid id)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Deleting category {CategoryId}", id);
        try
        {
            await _repository.Delete(userId, id, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error deleting category: {ErrorMessage}", ex.Message);
            throw;
        }
    }
}