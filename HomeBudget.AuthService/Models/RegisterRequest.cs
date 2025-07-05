using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class RegisterRequest
    {
        [JsonRequired]
        public required string Login { get; set; }
        [JsonRequired]
        public required string Email { get; set; }
        [JsonRequired]
        public required string FirstName { get; set; }
        [JsonRequired]
        public required string LastName { get; set; }
        [JsonRequired]
        public required string Password { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}
