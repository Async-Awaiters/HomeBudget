using HomeBudget.Directories.Data.Interfaces;
using HomeBudget.Directories.Data.Models;

namespace HomeBudget.Directories.Data.Repositories;

public class StubCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new()
    {
        new Category(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Food", "Groceries and dining out"),
        new Category(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Transport", "Public transport and taxis"),
        new Category(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Utilities", "Bills for housing utilities")
    };

    public Task<IEnumerable<Category>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult<IEnumerable<Category>>(_categories);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
    }

    public Task AddAsync(Category category, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var index = _categories.FindIndex(c => c.Id == category.Id);
        if (index >= 0)
        {
            _categories[index] = category;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _categories.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}