using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Services;
using HomeBudget.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;


namespace HomeBudget.AuthService.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/register", async (IUserService service, RegisterRequest request, CancellationToken ct) =>
        {
            var user = await service.RegisterAsync(request, ct);
            return Results.Ok(user);
        });

        app.MapPost("/api/login", async (IUserService service, LoginRequest request, HttpContext context, CancellationToken ct) =>
        {
            var token = await service.LoginAsync(request, ct);
            context.Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                MaxAge = TimeSpan.FromMinutes(60)
            });
            return Results.Ok(new { RedirectUrl = "/accounts" });
        });

        app.MapPut("/api/users", async (IUserService service, UpdateRequest request, HttpContext context, CancellationToken ct) =>
        {
            var userId = GetUserId(context);
            await service.UpdateAsync(userId, request, ct);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapPost("/api/logout", async (IUserService service, HttpContext context, CancellationToken ct) =>
        {
            await service.LogoutAsync(context);
            return Results.Ok(new { Message = "Logged out successfully" });
        }).RequireAuthorization();
    }

    private static Guid GetUserId(HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim);
    }
}