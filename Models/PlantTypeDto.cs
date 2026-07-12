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

    [JsonPropertyName("minSoilMoisture")]
    public int? MinSoilMoisture { get; set; }

    [JsonPropertyName("maxSoilMoisture")]
    public int? MaxSoilMoisture { get; set; }

    [JsonPropertyName("minTemp")]
    public decimal? MinTemp { get; set; }

    [JsonPropertyName("maxTemp")]
    public decimal? MaxTemp { get; set; }

    [JsonPropertyName("targetDli")]
    public decimal? TargetDli { get; set; }

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(NameLocal)
            ? NameLocal
            : Name ?? $"Biljka #{Id}";
}