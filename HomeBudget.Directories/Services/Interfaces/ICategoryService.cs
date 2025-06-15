using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.DTO;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(CreateCategoryDto category);
    Task<bool> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Guid id);
}