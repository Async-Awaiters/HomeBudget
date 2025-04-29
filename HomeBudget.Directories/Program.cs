using HomeBudget.Directories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Worker>();

// Заглушки для репозиториев
//TODO заменить на AddScoped при добавлении готовых репозиториев
builder.Services.AddSingleton<ICategoryRepository, StubCategoryRepository>();
builder.Services.AddSingleton<ICurrencyRepository, StubCurrencyRepository>();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Эндпоинты для категорий
app.MapGet("/api/categories", async (ICategoryRepository repository) =>
{
    var categories = await repository.GetAllAsync();
    return Results.Json(categories);
});

app.MapGet("/api/categories/{id:guid}", async (Guid id, ICategoryRepository repository) =>
{
    var category = await repository.GetByIdAsync(id);
    return category is not null ? Results.Json(category) : Results.NotFound();
});

app.MapPost("/api/categories", async (Category category, ICategoryRepository repository) =>
{
    // Генерация нового Guid если не указан
    category = category with { Id = category.Id == Guid.Empty ? Guid.NewGuid() : category.Id };
    await repository.AddAsync(category);
    return Results.Created($"/api/categories/{category.Id}", category);
});

app.MapPut("/api/categories/{id:guid}", async (Guid id, Category category, ICategoryRepository repository) =>
{
    if (id != category.Id)
        return Results.BadRequest("ID mismatch");

    await repository.UpdateAsync(category);
    return Results.NoContent();
});

app.MapDelete("/api/categories/{id:guid}", async (Guid id, ICategoryRepository repository) =>
{
    await repository.DeleteAsync(id);
    return Results.NoContent();
});

// Эндпоинты для валют
app.MapGet("/api/currencies", async (ICurrencyRepository repository) =>
{
    var currencies = await repository.GetAllAsync();
    return Results.Json(currencies);
});

app.MapGet("/api/currencies/{id:guid}", async (Guid id, ICurrencyRepository repository) =>
{
    var currency = await repository.GetByIdAsync(id);
    return currency is not null ? Results.Json(currency) : Results.NotFound();
});

app.Run();

//Тестовые модели и репозитории
//TODO Заменить на готовые репозитории
public record Category(Guid Id, string Name, string Description);
public record Currency(Guid Id, string Code, string Name);

// Интерфейсы репозиториев
public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Guid id);
}

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<Currency?> GetByIdAsync(Guid id);
}

// Заглушки для репозиториев
public class StubCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories = new()
    {
        new Category(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Food", "Groceries and dining out"),
        new Category(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Transport", "Public transport and taxis"),
        new Category(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Utilities", "Bills for housing utilities")
    };

    public Task<IEnumerable<Category>> GetAllAsync() => Task.FromResult<IEnumerable<Category>>(_categories);

    public Task<Category?> GetByIdAsync(Guid id) => Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));

    public Task AddAsync(Category category)
    {
        _categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category)
    {
        var index = _categories.FindIndex(c => c.Id == category.Id);
        if (index >= 0)
        {
            _categories[index] = category;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _categories.RemoveAll(c => c.Id == id);
        return Task.CompletedTask;
    }
}

public class StubCurrencyRepository : ICurrencyRepository
{
    private readonly List<Currency> _currencies = new()
    {
        new Currency(Guid.Parse("44444444-4444-4444-4444-444444444444"), "USD", "US Dollar"),
        new Currency(Guid.Parse("55555555-5555-5555-5555-555555555555"), "EUR", "Euro"),
        new Currency(Guid.Parse("66666666-6666-6666-6666-666666666666"), "GBP", "British Pound")
    };

    public Task<IEnumerable<Currency>> GetAllAsync() => Task.FromResult<IEnumerable<Currency>>(_currencies);

    public Task<Currency?> GetByIdAsync(Guid id) => Task.FromResult(_currencies.FirstOrDefault(c => c.Id == id));
}