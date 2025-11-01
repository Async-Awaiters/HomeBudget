using System.Text.Json.Serialization;

namespace AccountManagement.Models;

public class Valute
{
    [JsonPropertyName("ID")]
    public required string ID { get; set; }

    [JsonPropertyName("NumCode")]
    public required string NumCode { get; set; }

    [JsonPropertyName("CharCode")]
    public required string CharCode { get; set; }

    [JsonPropertyName("Nominal")]
    public int Nominal { get; set; }

    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    [JsonPropertyName("Value")]
    public decimal Value { get; set; }

    [JsonPropertyName("Previous")]
    public decimal Previous { get; set; }
}
