using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.DTO;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Categories>> GetAllCategoriesAsync();
    Task<Categories?> GetCategoryByIdAsync(Guid id);
    Task<Categories> CreateCategoryAsync(CreateCategoryDto category);
    Task<bool> UpdateCategoryAsync(Categories category);
    Task DeleteCategoryAsync(Guid id);
}