using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HomeBudget.Directories.Endpoints
{
    public static class CurrencyEndpoints
    {
        public static void MapCurrencyEndpoints(this WebApplication app)
        {
            // Эндпоинты для валют
            app.MapGet("/api/currencies", async (ICurrencyService service, HttpContext context) =>
            {
                IEnumerable<Currency> currencies = await service.GetAllCurrenciesAsync();
                return TypedResults.Ok(currencies);
            })
            .RequireAuthorization()
            .WithTags("Currencies")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получение полного списка валют",
                Description = "Возвращает полный список валют."
            });

            app.MapGet("/api/currencies/{id:guid}",
                async Task<Ok<Currency>> (Guid id, ICurrencyService service) =>
                {
                    Currency? currency = await service.GetCurrencyByIdAsync(id);
                    return TypedResults.Ok(currency);
                })
            .RequireAuthorization()
            .WithTags("Currencies")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получение конкретной валюты",
                Description = "Возвращает конкретную валюту."
            });
        }
    }
}
