using System.Text.Json.Serialization;

namespace HomeBudget.Directories.EF.DAL.Models
{
    public class Currency
    {
        public Guid Id { get; set; }

        [JsonRequired]
        public required string Name { get; set; }

        [JsonRequired]
        public required string Code { get; set; }

        [JsonRequired]
        public required string Country { get; set; }
    }
}
