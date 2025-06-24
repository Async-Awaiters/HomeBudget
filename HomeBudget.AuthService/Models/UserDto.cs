using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace HomeBudget.AuthService.Models
{
    public class UserDto
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
        public DateTime RegDate { get; set; }
        public DateOnly? BirthDate { get; set; }
    }
}
