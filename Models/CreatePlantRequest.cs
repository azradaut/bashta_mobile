using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class CreatePlantRequest
{
    [JsonPropertyName("potId")]
    public int PotId { get; set; }

    [JsonPropertyName("plantTypeId")]
    public int PlantTypeId { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("plantedAt")]
    public DateOnly PlantedAt { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}