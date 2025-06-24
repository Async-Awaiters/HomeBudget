using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.EF.Models
{
    [Table("Users")]
    public class User
    {
        [SwaggerIgnore]
        public Guid Id { get; set; }
        [JsonRequired]
        public required string Login { get; set; }
        [JsonRequired]
        public required string Email { get; set; }
        [JsonRequired]
        public required string FirstName { get; set; }
        [JsonRequired]
        public required string LastName { get; set; }
        [JsonRequired]
        public required string Password { get; set; } // Хэш пароля
        public DateTime RegDate { get; set; }
        public DateOnly? BirthDate { get; set; }
        [JsonRequired]
        public bool IsDeleted { get; set; }
    }
}
