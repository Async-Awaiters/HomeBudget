using HomeBudget.AuthService.CustomAttributes;
using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class UpdateRequest
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [CustomDateValidation(ErrorMessage = "Birth date cannot be in the future or before 1900-01-01.")]
        public DateOnly? BirthDate { get; set; }
    }
}
