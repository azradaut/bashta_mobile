using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class WateringEventDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("potId")]
    public int PotId { get; set; }

    [JsonPropertyName("triggeredBy")]
    public string? TriggeredBy { get; set; }

    [JsonPropertyName("durationSec")]
    public int? DurationSec { get; set; }

    [JsonPropertyName("amountMl")]
    public int? AmountMl { get; set; }

    [JsonPropertyName("soilMoistureBefore")]
    public int? SoilMoistureBefore { get; set; }

    [JsonPropertyName("soilMoistureAfter")]
    public int? SoilMoistureAfter { get; set; }

    [JsonPropertyName("skipped")]
    public bool Skipped { get; set; }

    [JsonPropertyName("skipReason")]
    public string? SkipReason { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}