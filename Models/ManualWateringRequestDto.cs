using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class ManualWateringRequestDto
{
    [JsonPropertyName("potId")]
    public int PotId { get; set; }

    [JsonPropertyName("durationSec")]
    public int DurationSec { get; set; } = 10;

    [JsonPropertyName("amountMl")]
    public int? AmountMl { get; set; } = 200;
}