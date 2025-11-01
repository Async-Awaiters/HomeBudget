using System.Text.Json.Serialization;

namespace AccountManagement.Models;

public class CurrencyData
{
    [JsonPropertyName("Date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("PreviousDate")]
    public DateTime PreviousDate { get; set; }

    [JsonPropertyName("PreviousURL")]
    public required string PreviousURL { get; set; }

    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("Valute")]
    public required Dictionary<string, Valute> Valute { get; set; }
}

