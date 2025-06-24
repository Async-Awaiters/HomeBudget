using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace HomeBudget.AuthService.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/register", async (IUserService service, RegisterRequest request) =>
        {
            UserDto user = await service.RegisterAsync(request);
            return TypedResults.Ok(user);
        });

        app.MapPost("/api/login", async (IUserService service, LoginRequest request, HttpContext context) =>
        {
            var token = await service.LoginAsync(request);
            context.Response.Headers.Append("Authorization", $"Bearer {token}");
            return TypedResults.Ok();
        });

        app.MapPut("/api/users", async (IUserService service, UpdateRequest request, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await service.UpdateAsync(userId, request);
            return TypedResults.Ok();
        }).RequireAuthorization();

        app.MapPost("/api/logout", async (IUserService service, HttpContext context) =>
        {
            await service.LogoutAsync(context);
            return TypedResults.Ok();
        }).RequireAuthorization();
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