using HomeBudget.Directories;
using HomeBudget.Directories.Data.Interfaces;
using HomeBudget.Directories.Data.Models;
using HomeBudget.Directories.Data.Repositories;
using HomeBudget.Directories.Services.Implementations;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();

// Регистрация сервисов
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();

// Регистрация заглушек репозиториев
builder.Services.AddScoped<ICategoryRepository, StubCategoryRepository>();
builder.Services.AddScoped<ICurrencyRepository, StubCurrencyRepository>();

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
app.MapGet("/api/categories", async (ICategoryService service, CancellationToken ct) =>
    TypedResults.Ok(await service.GetAllCategoriesAsync(ct)));

app.MapGet("/api/category/{id:guid}",
    async Task<Results<Ok<Category>, NotFound>>
        (Guid id, ICategoryService service, CancellationToken ct) =>
    {
        var category = await service.GetCategoryByIdAsync(id, ct);
        return category is not null
            ? TypedResults.Ok(category)
            : TypedResults.NotFound();
    });

app.MapPost("/api/category",
    async Task<Results<Created<Category>, ValidationProblem>>
        (Category category, ICategoryService service, CancellationToken ct) =>
    {
        var createdCategory = await service.CreateCategoryAsync(category, ct);
        return TypedResults.Created($"/api/category/{createdCategory.Id}", createdCategory);
    });

app.MapPut("/api/category/{id:guid}", async (
    Guid id,
    Category category,
    ICategoryService service,
    CancellationToken ct) =>
{
    if (id != category.Id)
        return Results.BadRequest("ID mismatch");

    await service.UpdateCategoryAsync(category, ct);
    return Results.NoContent();
});

app.MapDelete("/api/category/{id:guid}", async (
    Guid id,
    ICategoryService service,
    CancellationToken ct) =>
{
    await service.DeleteCategoryAsync(id, ct);
    return Results.NoContent();
});

app.MapGet("/api/currencies", async (
    ICurrencyService service,
    CancellationToken ct) =>
{
    var currencies = await service.GetAllCurrenciesAsync(ct);
    return TypedResults.Ok(currencies);
});

app.MapGet("/api/currency/{id:guid}",
    async Task<Results<Ok<Currency>, NotFound>>
        (Guid id, ICurrencyService service, CancellationToken ct) =>
    {
        var currency = await service.GetCurrencyByIdAsync(id, ct);
        return currency is not null
            ? TypedResults.Ok(currency)
            : TypedResults.NotFound();
    });

app.Run();