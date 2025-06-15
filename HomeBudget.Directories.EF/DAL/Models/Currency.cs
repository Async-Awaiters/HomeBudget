using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;

namespace HomeBudget.Directories.EF.DAL.Models
{
    public class Currency
    {
        [SwaggerIgnore]
        public Guid Id { get; set; }
        [JsonRequired]
        public string Name { get; set; }
        [JsonRequired]
        public string Code { get; set; }
        [JsonRequired]
        public string Country { get; set; }
    }
}
