using HomeBudget.AuthService.EF.Models;

namespace HomeBudget.AuthService.Security
{
    public interface ITokenBuilder
    {
        ITokenBuilder AddUserClaims(User user);
        ITokenBuilder SetIssuer(string issuer);
        ITokenBuilder SetAudience(string audience);
        ITokenBuilder SetLifetimeMinutes(int lifetimeMinutes);
        ITokenBuilder SetSigningKey(string secretKey);
        string Build();
    }
}
