using HomeBudget.Directories;
using HomeBudget.Directories.EF.DAL;
using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.EF.Exceptions;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<DirectoriesContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgreSQL")));

// Регистрация сервисов
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

// Регистрация заглушек репозиториев
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "HomeBudget Directories API",
            Version = "v1",
            Description = "API для работы со справочниками."
        });

        c.MapType<Guid>(() => new OpenApiSchema
        {
            Type = "string",
            Format = "uuid",
            Example = OpenApiAnyFactory.CreateFromJson("\"00000000-0000-0000-0000-000000000000\"")
        });
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => { options.RouteTemplate = "/openapi/{documentName}.json"; });
    app.MapScalarApiReference(options =>
    {
        options.Title = "HomeBudget API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}

app.UseHttpsRedirection();

// Эндпоинты для категорий

app.MapGet("/api/categories", async (ICategoryService service) =>
{
    var categories = await service.GetAllCategoriesAsync();
    return TypedResults.Ok(categories); // Явно возвращаем пустой массив, если categories равно null
})
.WithTags("Categories")
.WithOpenApi(operation => new(operation)
{
    Summary = "Получение всех категорий",
    Description = "Возвращает все категории или пустой массив, если список категорий пуст."
});

app.MapGet("/api/categories/{id:guid}",
    async Task<Results<Ok<Category>, NotFound>> (Guid id, ICategoryService service) =>
    {
        var category = await service.GetCategoryByIdAsync(id);
        return category is not null
            ? TypedResults.Ok(category)
            : TypedResults.NotFound();
    })
.WithTags("Categories")
.WithOpenApi(operation =>
{
    operation.Summary = "Получение категории по идентификатору";
    operation.Description = "Возвращает категорию по индентификатору.";

    return operation;
});

app.MapPost("/api/categories",
    async Task<Results<Created<Category>, BadRequest<string>>> (Category category, ICategoryService service) =>
    {
        Category createdCategory;
        try
        {
            createdCategory = await service.CreateCategoryAsync(category);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }

        return TypedResults.Created($"/api/category/{createdCategory.Id}", createdCategory);
    })
.WithTags("Categories")
.WithOpenApi(operation => new(operation)
{
    Summary = "Создание категории",
    Description = "Добавляет новую категорию."
});

app.MapPut("/api/categories/{id:guid}",
    async Task<Results<Ok, NotFound, ValidationProblem, BadRequest<string>>>
        (Guid id, Category category, ICategoryService service) =>
    {
        // Проверка соответствия ID в маршруте и теле запроса
        if (id != category.Id)
        {
            return TypedResults.BadRequest("ID in route doesn't match ID in body");
        }

        try
        {
            await service.UpdateCategoryAsync(category);
        }
        catch (EntityNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }

        return TypedResults.Ok();
    })
.WithTags("Categories")
.WithOpenApi(operation => new(operation)
{
    Summary = "Обновление заданной категории",
    Description = "Обновляет категорию."
});

app.MapDelete("/api/categories/{id:guid}", async Task<Results<Ok, NotFound, ValidationProblem, BadRequest<string>>> (Guid id, ICategoryService service) =>
{
    try
    {
        await service.DeleteCategoryAsync(id);
    }
    catch (EntityNotFoundException)
    {
        return TypedResults.NotFound();
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }

    return TypedResults.Ok();
})
.WithTags("Categories")
.WithOpenApi(operation => new(operation)
{
    Summary = "Удаление заданной категории",
    Description = "Удаляет категорию."
});

// Эндпоинты для валют

app.MapGet("/api/currencies", async (ICurrencyService service) =>
{
    IEnumerable<Currency> currencies = await service.GetAllCurrenciesAsync();
    return TypedResults.Ok(currencies);
})
.WithTags("Currencies")
.WithOpenApi(operation => new(operation)
{
    Summary = "Получение полного списка валют",
    Description = "Возвращает полный список валют."
});

app.MapGet("/api/currencies/{id:guid}",
    async Task<Results<Ok<Currency>, NotFound>> (Guid id, ICurrencyService service) =>
    {
        Currency? currency = await service.GetCurrencyByIdAsync(id);
        return currency is not null
            ? TypedResults.Ok(currency)
            : TypedResults.NotFound();
    })
.WithTags("Currencies")
.WithOpenApi(operation => new(operation)
{
    Summary = "Получение конкретной валюты",
    Description = "Возвращает конкретную валюту."
});

app.Run();