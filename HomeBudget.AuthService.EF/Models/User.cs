using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.EF.Models
{
    [Table("Users")]
    public class User
    {
        [SwaggerIgnore]
        public Guid Id { get; set; }
        [Required]
        public required string Login { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Password { get; set; } // Хэш пароля
        public DateTime RegDate { get; set; }
        public DateOnly? BirthDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
