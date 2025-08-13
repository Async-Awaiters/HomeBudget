using HomeBudget.Directories.Models.Categories.Requests;
using HomeBudget.Directories.Models.Categories.Responses;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace HomeBudget.Directories.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this WebApplication app)
        {
            // Эндпоинты для категорий
            app.MapGet("/api/categories", async (ICategoryService service, HttpContext context) =>
            {
                var userId = GetUserId(context);
                var categories = await service.GetAllCategoriesAsync(userId);
                return TypedResults.Ok(categories); // Явно возвращаем пустой массив, если categories равно null
            })
            .RequireAuthorization()
            .WithTags("Categories")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получение всех категорий",
                Description = "Возвращает все категории или пустой массив, если список категорий пуст."
            });

            app.MapGet("/api/categories/{id:guid}",
                async Task<Results<Ok<CategoryResponse>, NotFound>> (Guid id, ICategoryService service, HttpContext context) =>
                {
                    var userId = GetUserId(context);
                    var category = await service.GetCategoryByIdAsync(userId, id);
                    return category is not null
                        ? TypedResults.Ok(category)
                        : TypedResults.NotFound();
                })
            .RequireAuthorization()
            .WithTags("Categories")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Получение категории по идентификатору";
                operation.Description = "Возвращает категорию по индентификатору.";

                return operation;
            });

            app.MapPost("/api/categories",
                async Task<Created<CategoryResponse>> (CreateCategoryRequest categoryRequest, ICategoryService service, HttpContext context) =>
                {
                    var userId = GetUserId(context);
                    var createdCategory = await service.CreateCategoryAsync(userId, categoryRequest);
                    return TypedResults.Created($"/api/category/{createdCategory.Id}", createdCategory);
                })
            .RequireAuthorization()
            .WithTags("Categories")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание категории",
                Description = "Добавляет новую категорию."
            });

            app.MapPut("/api/categories/{id:guid}",
                async Task<Ok<CategoryResponse>>
                    (Guid id, UpdateCategoryRequest category, ICategoryService service, HttpContext context) =>
                {
                    var userId = GetUserId(context);
                    var updated = await service.UpdateCategoryAsync(userId, id, category);
                    return TypedResults.Ok(updated);
                })
            .RequireAuthorization()
            .WithTags("Categories")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Обновление заданной категории",
                Description = "Обновляет категорию."
            });

            app.MapDelete("/api/categories/{id:guid}", async Task<Ok> (Guid id, ICategoryService service, HttpContext context) =>
            {
                var userId = GetUserId(context);
                await service.DeleteCategoryAsync(userId, id);
                return TypedResults.Ok();
            })
            .RequireAuthorization()
            .WithTags("Categories")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Удаление заданной категории",
                Description = "Удаляет категорию."
            });
        }

        private static Guid GetUserId(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID format in token.");
            }

            return userId;
        }
    }
}
