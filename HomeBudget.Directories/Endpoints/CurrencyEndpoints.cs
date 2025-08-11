using HomeBudget.Directories.EF.DAL.Models;
using HomeBudget.Directories.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace HomeBudget.Directories.Endpoints
{
    public static class CurrencyEndpoints
    {
        public static void MapCurrencyEndpoints(this WebApplication app)
        {
            // Эндпоинты для валют
            app.MapGet("/api/currencies", async (ICurrencyService service, HttpContext context) =>
            {
                var userId = GetUserId(context);
                IEnumerable<Currency> currencies = await service.GetAllCurrenciesAsync(userId);
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
                async Task<Results<Ok<Currency>, NotFound>> (Guid id, ICurrencyService service) =>
                {
                    Currency? currency = await service.GetCurrencyByIdAsync(id);
                    return currency is not null
                        ? TypedResults.Ok(currency)
                        : TypedResults.NotFound();
                })
            .RequireAuthorization()
            .WithTags("Currencies")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получение конкретной валюты",
                Description = "Возвращает конкретную валюту."
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
