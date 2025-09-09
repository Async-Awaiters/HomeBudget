using HomeBudget.AuthService.EF.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HomeBudget.AuthService.Security
{
    public class JwtTokenBuilder : ITokenBuilder
    {
        private readonly List<Claim> _claims = new();
        private string? _issuer;
        private string? _audience;
        private int _lifetimeMinutes;
        private string? _secretKey;

        public ITokenBuilder AddUserClaims(User user)
        {
            _claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            _claims.Add(new Claim(ClaimTypes.Name, user.Login));

            if (!string.IsNullOrEmpty(user.Email))
                _claims.Add(new Claim(ClaimTypes.Email, user.Email));

            return this;
        }

        public ITokenBuilder SetIssuer(string issuer)
        {
            _issuer = issuer;
            return this;
        }

        public ITokenBuilder SetAudience(string audience)
        {
            _audience = audience;
            return this;
        }

        public ITokenBuilder SetLifetimeMinutes(int lifetimeMinutes)
        {
            _lifetimeMinutes = lifetimeMinutes;
            return this;
        }

        public ITokenBuilder SetSigningKey(string secretKey)
        {
            _secretKey = secretKey;
            return this;
        }

        public string Build()
        {
            if (string.IsNullOrEmpty(_secretKey))
                throw new InvalidOperationException("Secret key must be provided.");
            if (_lifetimeMinutes <= 0)
                throw new InvalidOperationException("Token lifetime must be greater than zero.");
            if (string.IsNullOrEmpty(_issuer) || string.IsNullOrEmpty(_audience))
                throw new InvalidOperationException("Issuer and Audience must be provided.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: _claims,
                expires: DateTime.UtcNow.AddMinutes(_lifetimeMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
