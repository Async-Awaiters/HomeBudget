using HomeBudget.AuthService.CustomAttributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class RegisterRequest
    {
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Login must be between 1 and 100 characters.")]
        public required string Login { get; set; }
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Email must be between 1 and 100 characters.")]
        public required string Email { get; set; }
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "FirstName must be between 1 and 100 characters.")]
        public required string FirstName { get; set; }
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "LastName must be between 1 and 100 characters.")]
        public required string LastName { get; set; }
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 100 characters.")]
        public required string Password { get; set; }
        [CustomDateValidation(ErrorMessage = "Birth date cannot be in the future or before 1900-01-01.")]
        public DateOnly? BirthDate { get; set; }
    }
}
