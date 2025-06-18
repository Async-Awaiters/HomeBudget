using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace HomeBudget.Directories.EF.DAL.Models
{
    public class Currency
    {
        [SwaggerIgnore]
        public Guid Id { get; set; }

        [JsonRequired]
        public required string Name { get; set; }

        [JsonRequired]
        public required string Code { get; set; }

        [JsonRequired]
        public required string Country { get; set; }
    }
}
