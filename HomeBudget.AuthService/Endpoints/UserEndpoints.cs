using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Services.Interfaces;
using HomeBudget.AuthService.ValidationHelpers.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HomeBudget.AuthService.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/register", async (IUserService service, RegisterRequest request, IRequestValidator<RegisterRequest> validator) =>
        {
            validator.Validate(request);

            RegisterResponse response = await service.RegisterAsync(request);
            return TypedResults.Created($"/api/users/{response.User.Id}" ,response);
        })
        .AllowAnonymous()
        .WithTags("Authentication")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Регистрация нового пользователя",
            Description = "Регистрирует пользователя и возвращает его представление."
        });

        app.MapPost("/api/login", async (IUserService service, LoginRequest request, HttpContext context, IRequestValidator<LoginRequest> validator) =>
        {
            validator.Validate(request);

            var (response, token) = await service.LoginAsync(request);

            context.Response.Headers.Append("Authorization", $"Bearer {token}");
            return TypedResults.Ok(response);
        })
        .AllowAnonymous()
        .WithTags("Authentication")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Логин",
            Description = "Возвращает JWT в заголовке ответа."
        });

        app.MapPut("/api/users", async (IUserService service, UpdateRequest request, HttpContext context, IUpdateRequestValidator<UpdateRequest> validator) =>
        {
            var validFields = validator.Validate(request);

            var userId = GetUserId(context);
            await service.UpdateAsync(userId, request, validFields);
            return TypedResults.Ok();
        })
        .RequireAuthorization()
        .WithTags("Users")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Изменение данных пользователя",
            Description = "Обновляет данные пользователя."
        });

        app.MapGet("/api/refresh", async (IUserService service, HttpContext context) =>
        {
            var userId = GetUserId(context);

            var newToken = await service.RefreshTokenAsync(userId);
            context.Response.Headers.Append("Authorization", $"Bearer {newToken}");
            return TypedResults.Ok();
        })
        .RequireAuthorization()
        .WithTags("Refresh")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Обновление токена",
            Description = "Возвращает новый токен в заголовке."
        });

        app.MapPost("/api/logout", async (IUserService service, HttpContext context) =>
        {
            await service.LogoutAsync(context);
            return TypedResults.Ok();
        })
        .RequireAuthorization()
        .WithTags("Authentication")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Логаут",
            Description = "Разлогинивает пользователя."
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