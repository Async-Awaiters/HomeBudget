using HomeBudget.Directories.EF.DAL.Models;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Categories>> GetAllCategoriesAsync();
    Task<Categories?> GetCategoryByIdAsync(Guid id);
    Task<Categories> CreateCategoryAsync(Categories category);
    Task UpdateCategoryAsync(Categories category);
    Task DeleteCategoryAsync(Guid id);
}