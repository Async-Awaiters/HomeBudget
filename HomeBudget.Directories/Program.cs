using HomeBudget.Directories;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using HomeBudget.Directories.EF.DAL.Interfaces;
using HomeBudget.Directories.EF.DAL;
using HomeBudget.Directories.EF.DAL.Models;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddOptions<ServiceTimeoutsOptions>()
    .Bind(builder.Configuration.GetSection("Services:Timeouts"))
    .Validate(x => x.CategoryService > 0, "CategoryService timeout must be positive")
    .Validate(x => x.CurrencyService > 0, "CurrencyService timeout must be positive")
    .Validate(x => x.CategoryService <= 60_000, "CategoryService timeout too long")
    .Validate(x => x.CurrencyService <= 60_000, "CurrencyService timeout too long");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
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
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });
    app.MapScalarApiReference(options =>
    {
        options.Title = "HomeBudget API";
        options.ShowSidebar = true;
    });
}

app.UseHttpsRedirection();

// Эндпоинты для категорий
app.MapGet("/api/categories", async (ICategoryService service) =>
{
    var categories = await service.GetAllCategoriesAsync();
    return TypedResults.Ok(categories ?? Enumerable.Empty<Categories>()); // Явно возвращаем пустой массив
});

app.MapGet("/api/category/{id:guid}",
    async Task<Results<Ok<Categories>, NotFound>>
        (Guid id, ICategoryService service) =>
    {
        var category = await service.GetCategoryByIdAsync(id);
        return category is not null
            ? TypedResults.Ok(category)
            : TypedResults.NotFound();
    });

app.MapPost("/api/category",
    async Task<Results<Created<Categories>, ValidationProblem>>
        (Categories category, ICategoryService service) =>
    {
        var createdCategory = await service.CreateCategoryAsync(category);
        return TypedResults.Created($"/api/category/{createdCategory.Id}", createdCategory);
    });

app.MapPut("/api/category/{id:guid}",
    async Task<Results<NoContent, NotFound, ValidationProblem, BadRequest<string>>>
        (Guid id, Categories category, ICategoryService service) =>
    {
        // Проверка соответствия ID в маршруте и теле запроса
        if (id != category.Id)
        {
            return TypedResults.BadRequest("ID in route doesn't match ID in body");
        }

        var updateResult = await service.UpdateCategoryAsync(category);

        return updateResult
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    });

app.MapDelete("/api/category/{id:guid}", async (
    Guid id,
    ICategoryService service) =>
{
    await service.DeleteCategoryAsync(id);
    return TypedResults.NoContent();
});

app.MapGet("/api/currencies", async (
    ICurrencyService service) =>
{
    var currencies = await service.GetAllCurrenciesAsync();
    return TypedResults.Ok(currencies);
});

app.MapGet("/api/currency/{id:guid}",
    async Task<Results<Ok<Currency>, NotFound>>
        (Guid id, ICurrencyService service) =>
    {
        var currency = await service.GetCurrencyByIdAsync(id);
        return currency is not null
            ? TypedResults.Ok(currency)
            : TypedResults.NotFound();
    });

app.Run();