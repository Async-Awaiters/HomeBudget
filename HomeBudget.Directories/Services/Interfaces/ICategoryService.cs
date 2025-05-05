using HomeBudget.Directories.Data.Models;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(CancellationToken ct);
    Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken ct);
    Task<Category> CreateCategoryAsync(Category category, CancellationToken ct);
    Task UpdateCategoryAsync(Category category, CancellationToken ct);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct);
}