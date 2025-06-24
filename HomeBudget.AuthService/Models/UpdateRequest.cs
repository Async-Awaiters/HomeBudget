using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class UpdateRequest
    {
        [JsonRequired]
        public required string Email { get; set; }
        [JsonRequired]
        public required string FirstName { get; set; }
        [JsonRequired]
        public required string LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}
