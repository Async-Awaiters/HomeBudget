using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync(Guid userId);
    Task<Category?> GetCategoryByIdAsync(Guid userId, Guid id);
    Task<Category> CreateCategoryAsync(Guid UserId, Category category);
    Task UpdateCategoryAsync(Guid userId, Guid Id, Category category);
    Task DeleteCategoryAsync(Guid userId, Guid id);
}