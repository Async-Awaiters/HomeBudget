using HomeBudget.AuthService.Models;
using Microsoft.AspNetCore.Http;

namespace HomeBudget.AuthService.Services.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<string> LoginAsync(LoginRequest request);
        Task UpdateAsync(Guid userId, UpdateRequest request, Dictionary<string, object?> validFields);
        Task LogoutAsync(HttpContext context);
        Task<string> RefreshTokenAsync(Guid userID);
    }
}
