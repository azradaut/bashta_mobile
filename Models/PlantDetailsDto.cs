using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class PlantDetailsDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("potId")]
    public int PotId { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("plantedAt")]
    public DateTime? PlantedAt { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("plantType")]
    public PlantTypeDto? PlantType { get; set; }
}