using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class PlantTypeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("nameLocal")]
    public string? NameLocal { get; set; }

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(NameLocal)
            ? NameLocal
            : Name ?? $"Biljka #{Id}";
}