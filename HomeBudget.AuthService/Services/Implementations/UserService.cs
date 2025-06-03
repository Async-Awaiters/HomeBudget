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

namespace HomeBudget.AuthService.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository repository, ILogger<UserService> logger, IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = request.Login,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RegDate = DateTime.UtcNow,
                BirthDate = request.BirthDate,
                IsDeleted = false
            };

            await _repository.AddAsync(user, ct);
            _logger.LogInformation("User registered: {Login}", user.Login);

            return MapToDto(user);
        }

        public async Task<string> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            var user = await _repository.GetByLoginAsync(request.Login, ct);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid login or password");
            }

            return GenerateJwtToken(user);
        }

        public async Task UpdateAsync(Guid userId, UpdateRequest request, CancellationToken ct)
        {
            var user = await _repository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            user.Email = request.Email ?? user.Email;
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.BirthDate = request.BirthDate ?? user.BirthDate;

            await _repository.UpdateAsync(user, ct);
            _logger.LogInformation("User updated: {Login}", user.Login);
        }

        public Task LogoutAsync(HttpContext context)
        {
            context.Response.Cookies.Delete("auth_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
            return Task.CompletedTask;
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
    }
    }
