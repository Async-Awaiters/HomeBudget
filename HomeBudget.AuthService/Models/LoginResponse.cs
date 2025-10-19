using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class LoginResponse
    {
        [JsonPropertyName("user")]
        public required UserData User { get; set; }
    }
}
