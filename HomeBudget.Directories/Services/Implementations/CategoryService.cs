﻿using HomeBudget.Directories.Services.Interfaces;
using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.Extensions.Options;
using HomeBudget.Directories.Services.DTO;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace HomeBudget.Directories.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoriesRepository _repository;
    private readonly ILogger<CategoryService> _logger;
    private readonly TimeSpan _timeout;
    private const int _defaultTimeout = 30000;

    public CategoryService(
            ICategoriesRepository repository,
            ILogger<CategoryService> logger,
            IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;
        int timeoutMs = configuration.GetValue<int>("Services:Timeouts:CategoryService", _defaultTimeout);
        _timeout = TimeSpan.FromMilliseconds(timeoutMs);
    }

    public async Task<IEnumerable<Categories>> GetAllCategoriesAsync()
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Getting all categories");
        return await _repository.GetAll(cts.Token).ToListAsync();
    }

    public async Task<Categories?> GetCategoryByIdAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogDebug("Getting category by ID: {CategoryId}", id);
        return await _repository.GetById(id, cts.Token);
    }

    public async Task<Categories> CreateCategoryAsync(CreateCategoryDto categoryDto)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Creating new category");

        var category = new Categories
        {
            Name = categoryDto.Name.Trim(),
            ParentId = categoryDto.ParentId,
            UserId = categoryDto.UserId,
            IsDeleted = false
        };

        await _repository.Create(category, cts.Token);
        return category;
    }

    public async Task<bool> UpdateCategoryAsync(Categories category)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Updating category {CategoryId}", category.Id);
        var updated = await _repository.Update(category, cts.Token);
        return updated;
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        using var cts = new CancellationTokenSource(_timeout);
        _logger.LogInformation("Deleting category {CategoryId}", id);
        await _repository.Delete(id, cts.Token);
    }
}