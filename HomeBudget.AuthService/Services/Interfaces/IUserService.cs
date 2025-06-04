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
        Task<UserDto> RegisterAsync(RegisterRequest request);
        Task<string> LoginAsync(LoginRequest request);
        Task UpdateAsync(Guid userId, UpdateRequest request);
        Task LogoutAsync(HttpContext context);
    }
}
