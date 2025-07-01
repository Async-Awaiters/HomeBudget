using HomeBudget.AuthService.EF.Models;
using HomeBudget.AuthService.EF.Repositories.Interfaces;
using HomeBudget.AuthService.Exceptions;
using HomeBudget.AuthService.Models;
using HomeBudget.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HomeBudget.AuthService.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _timeout;
        private const int _defaultTimeout = 30000;

        public UserService(IUserRepository repository, ILogger<UserService> logger, IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
            int timeoutMs = configuration.GetValue("Services:Timeouts:UserService", _defaultTimeout);
            _timeout = TimeSpan.FromMilliseconds(timeoutMs);
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            using var cts = new CancellationTokenSource(_timeout);

            try
            {
                var existingUserByLogin = await _repository.GetByLoginAsync(request.Login, cts.Token);
                if (existingUserByLogin != null)
                {
                    _logger.LogWarning("Registration failed: Login '{Login}' is already taken.", request.Login);
                    throw new DuplicateUserException(nameof(request.Login), request.Login);
                }

                var existingUserByEmail = await _repository.GetByEmailAsync(request.Email, cts.Token);
                if (existingUserByEmail != null)
                {
                    _logger.LogWarning("Registration failed: Email '{Email}' is already taken.", request.Email);
                    throw new DuplicateUserException(nameof(request.Email), request.Email);
                }

                var user = new User
                {
                    Login = request.Login,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    RegDate = DateTime.UtcNow,
                    BirthDate = request.BirthDate,
                };

                await _repository.AddUserAsync(user, cts.Token);
                _logger.LogInformation("User registered: {Login}", user.Login);

                var response = new RegisterResponse
                {
                    Success = true,
                    User = new UserData
                    {
                        Id = user.Id,
                        Login = user.Login,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        RegDate = user.RegDate,
                        BirthDate = user.BirthDate
                    }
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register user with login: {Login}", request.Login);
                throw; // Перебрасываем исключение для обработки middleware
            }
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {
            using var cts = new CancellationTokenSource(_timeout);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to login user: {Login}", request.Login);
                throw;
            }
        }

        public async Task UpdateAsync(Guid userId, UpdateRequest request)
        {
            using var cts = new CancellationTokenSource(_timeout);

            try
            {
                var user = await _repository.GetByIdAsync(userId, cts.Token);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    throw new KeyNotFoundException("User not found");
                }

                if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;
                if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
                if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
                if (request.BirthDate.HasValue) user.BirthDate = request.BirthDate;

                await _repository.UpdateUserAsync(user, cts.Token);
                _logger.LogInformation("User updated: {Login}", user.Login);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with ID: {UserId}", userId);
                throw;
            }
        }

        //Пока сещуствует как заглушка
        public Task LogoutAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation("User logged out successfully");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to logout user");
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            try
            {
                var jwtSecret = _configuration["Jwt:Secret"];
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    throw new InvalidOperationException("JWT Secret is not configured. Please set 'Jwt:Secret' in appsettings.json.");
                }

                var lifetimeMinutes = _configuration["Jwt:LifetimeMinutes"];
                if (string.IsNullOrEmpty(lifetimeMinutes) || !int.TryParse(lifetimeMinutes, out var lifetime))
                {
                    throw new InvalidOperationException("JWT LifetimeMinutes is not configured or invalid. Please set 'Jwt:LifetimeMinutes' in appsettings.json.");
                }

                var jwtIssuer = _configuration["Jwt:Issuer"];
                if (string.IsNullOrEmpty(jwtIssuer))
                {
                    throw new InvalidOperationException("JWT Issuer is not configured. Please set 'Jwt:Issuer' in appsettings.json.");
                }

                var jwtAudience = _configuration["Jwt:Audience"];
                if (string.IsNullOrEmpty(jwtAudience))
                {
                    throw new InvalidOperationException("JWT Audience is not configured. Please set 'Jwt:Audience' in appsettings.json.");
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(jwtSecret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Login)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(lifetime),
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
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
