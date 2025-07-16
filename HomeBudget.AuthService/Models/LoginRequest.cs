using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class LoginRequest
    {
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Login must be between 1 and 100 characters.")]
        public required string Login { get; set; }
        [JsonRequired]
        [Required]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 100 characters.")]
        public required string Password { get; set; }
    }
}
