using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class LoginRequest
    {
        [JsonRequired]
        public required string Login { get; set; }
        [JsonRequired]
        public required string Password { get; set; }
    }
}
