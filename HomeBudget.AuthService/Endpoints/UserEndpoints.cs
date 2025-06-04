using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;


namespace HomeBudget.AuthService.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/api/register", async (IUserService service, RegisterRequest request) =>
        {
            var user = await service.RegisterAsync(request);
            return Results.Ok(user);
        });

        app.MapPost("/api/login", async (IUserService service, LoginRequest request, HttpContext context) =>
        {
            var token = await service.LoginAsync(request);
            context.Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                MaxAge = TimeSpan.FromMinutes(60)
            });
            return Results.Ok(new { Token = token });
        });

        app.MapPut("/api/users", async (IUserService service, UpdateRequest request, HttpContext context) =>
        {
            var userId = GetUserId(context);
            await service.UpdateAsync(userId, request);
            return Results.NoContent();
        }).RequireAuthorization();

        app.MapPost("/api/logout", async (IUserService service, HttpContext context) =>
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