using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class SensorReadingDto
{
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("potId")]
    public int PotId { get; set; }

    [JsonPropertyName("soilMoisture")]
    public decimal? SoilMoisture { get; set; }

    [JsonPropertyName("temperature")]
    public decimal? Temperature { get; set; }

    [JsonPropertyName("humidity")]
    public decimal? Humidity { get; set; }

    [JsonPropertyName("lux")]
    public decimal? Lux { get; set; }
}