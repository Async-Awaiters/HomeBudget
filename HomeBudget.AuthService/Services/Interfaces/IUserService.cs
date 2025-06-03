using HomeBudget.AuthService.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HomeBudget.AuthService.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct);
        Task<string> LoginAsync(LoginRequest request, CancellationToken ct);
        Task UpdateAsync(Guid userId, UpdateRequest request, CancellationToken ct);
        Task LogoutAsync(HttpContext context);
    }
}
