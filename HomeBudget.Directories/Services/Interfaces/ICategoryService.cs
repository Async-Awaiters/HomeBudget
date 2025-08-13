using HomeBudget.Directories.Models.Categories.Requests;
using HomeBudget.Directories.Models.Categories.Responses;

namespace HomeBudget.Directories.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync(Guid userId);
    Task<CategoryResponse?> GetCategoryByIdAsync(Guid userId, Guid id);
    Task<CategoryResponse> CreateCategoryAsync(Guid UserId, CreateCategoryRequest category);
    Task<CategoryResponse> UpdateCategoryAsync(Guid userId, Guid Id, UpdateCategoryRequest category);
    Task DeleteCategoryAsync(Guid userId, Guid id);
}