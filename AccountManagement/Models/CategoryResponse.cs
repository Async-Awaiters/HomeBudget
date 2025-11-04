using System.Text.Json.Serialization;

namespace AccountManagement.Models;

public class CategoryResponse
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
}
