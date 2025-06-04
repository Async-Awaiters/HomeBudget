using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Services.Interfaces;
using HomeBudget.AuthService.EF.Repositories.Interfaces;
using HomeBudget.AuthService.EF.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace HomeBudget.AuthService.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _defaultTimeout;


        public UserService(IUserRepository repository, ILogger<UserService> logger, IConfiguration configuration, IOptions<ServiceTimeoutsOptions> options)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
            _defaultTimeout = TimeSpan.FromMilliseconds(options.Value.UserService);
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var user = new User
                {
                    Login = request.Login,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    RegDate = DateTime.UtcNow,
                    BirthDate = request.BirthDate,
                    IsDeleted = false
                };

                using var cts = new CancellationTokenSource(_defaultTimeout);
                await _repository.AddAsync(user, cts.Token);
                _logger.LogInformation("User registered: {Login}", user.Login);

                return MapToDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user with login: {Login}", request.Login);
                throw; // Перебрасываем исключение для обработки middleware
            }

        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            using var cts = new CancellationTokenSource(_defaultTimeout);

            try
            {
                _logger.LogDebug("Attempting login for user: {Login}", request.Login);
                var user = await _repository.GetByLoginAsync(request.Login, cts.Token);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                {
                    _logger.LogWarning("Failed login attempt for user: {Login}", request.Login);
                    throw new UnauthorizedAccessException("Invalid login or password");
                }

                var token = GenerateJwtToken(user);
                _logger.LogInformation("User logged in: {Login}", user.Login);
                return token;
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Уже обработано в if выше, просто перебрасываем
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to login user: {Login}", request.Login);
                throw;
            }
        }

        public async Task UpdateAsync(Guid userId, UpdateRequest request)
        {
            using var cts = new CancellationTokenSource(_defaultTimeout);

            try
            {
                var user = await _repository.GetByIdAsync(userId, cts.Token);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    throw new KeyNotFoundException("User not found");
                }

                user.Email = request.Email ?? user.Email;
                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.BirthDate = request.BirthDate ?? user.BirthDate;

                await _repository.UpdateAsync(user, cts.Token);
                _logger.LogInformation("User updated: {Login}", user.Login);
            }
            catch (KeyNotFoundException)
            {
                throw; // Уже обработано в if выше, просто перебрасываем
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with ID: {UserId}", userId);
                throw;
            }
        }

        public Task LogoutAsync(HttpContext context)
        {
            try
            {
                context.Response.Cookies.Delete("auth_token", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });
                _logger.LogInformation("User logged out successfully");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to logout user");
                throw;
            }
        }

        private UserDto MapToDto(User user) => new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RegDate = user.RegDate,
            BirthDate = user.BirthDate
        };

        private string GenerateJwtToken(User user)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Login)
                }),
                    Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:LifetimeMinutes"])),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate JWT token for user: {Login}", user.Login);
                throw;
            }
        }
    }
}
