using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.EF.Exceptions;
using HomeBudget.Directories.Models.Categories.Requests;
using HomeBudget.Directories.Models.Categories.Responses;
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

    public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync(Guid userId)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Getting all categories");

        var categories = await _repository
        .GetAll(userId, cts.Token)
        .Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            ParentId = c.ParentId
        })
        .ToListAsync(cts.Token);

        return categories;
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid userId, Guid id)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogDebug("Getting category by ID: {CategoryId}", id);

        var category = await _repository.GetById(userId, id, cts.Token);
        return category is null ? null : MapToResponse(category);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(Guid userId, CreateCategoryRequest categoryRequest)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Creating new category");

        try
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = categoryRequest.Name,
                ParentId = categoryRequest.ParentId,
                UserId = userId,
                IsDeleted = false
            };

            await _repository.Create(category, cts.Token);

            return MapToResponse(category);
        }

        catch (Exception ex)
        {
            _logger.LogError("Error creating category: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid userId, Guid id, UpdateCategoryRequest request)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Updating category {CategoryId}", id);
        try
        {
            var category = await _repository.GetById(userId, id, cts.Token);
            if (category is null)
                throw new EntityNotFoundException("Категория не найдена");

            if (!string.IsNullOrWhiteSpace(request.Name)) category.Name = request.Name;
            if (request.ParentId.HasValue) category.ParentId = request.ParentId;

            await _repository.Update(userId, id, category, cts.Token);

            return MapToResponse(category);
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

    private static CategoryResponse MapToResponse(Category category) =>
        new CategoryResponse
        {
        Id = category.Id,
        Name = category.Name,
        ParentId = category.ParentId
        };
}