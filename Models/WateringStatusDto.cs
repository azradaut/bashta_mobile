using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class WateringStatusDto
{
    [JsonPropertyName("potId")]
    public int PotId { get; set; }

    [JsonPropertyName("plantId")]
    public int? PlantId { get; set; }

    [JsonPropertyName("plantName")]
    public string? PlantName { get; set; }

    [JsonPropertyName("currentSoilMoisture")]
    public decimal? CurrentSoilMoisture { get; set; }

    [JsonPropertyName("lastSensorReadingAt")]
    public DateTime? LastSensorReadingAt { get; set; }

    [JsonPropertyName("minRecommendedSoilMoisture")]
    public int? MinRecommendedSoilMoisture { get; set; }

    [JsonPropertyName("maxRecommendedSoilMoisture")]
    public int? MaxRecommendedSoilMoisture { get; set; }

    [JsonPropertyName("lastWateredAt")]
    public DateTime? LastWateredAt { get; set; }

    [JsonPropertyName("wateringCountLast24h")]
    public int WateringCountLast24h { get; set; }

    [JsonPropertyName("maxWateringCountLast24h")]
    public int MaxWateringCountLast24h { get; set; }

    [JsonPropertyName("remainingWateringsLast24h")]
    public int RemainingWateringsLast24h { get; set; }

    [JsonPropertyName("recommendedAmountMl")]
    public int RecommendedAmountMl { get; set; }

    [JsonPropertyName("canWater")]
    public bool CanWater { get; set; }

    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; set; }

    [JsonPropertyName("warningMessage")]
    public string? WarningMessage { get; set; }

    [JsonPropertyName("recentEvents")]
    public List<WateringEventDto> RecentEvents { get; set; } = [];
}