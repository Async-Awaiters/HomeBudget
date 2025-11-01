using System.Text.Json.Serialization;

namespace AccountManagement.Models;

public class CurrencyIdsData
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("code")]
    public required string Code { get; set; }
    [JsonPropertyName("country")]
    public required string Country { get; set; }
}
