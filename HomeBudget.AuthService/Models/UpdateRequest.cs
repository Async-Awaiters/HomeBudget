using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class UpdateRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}
