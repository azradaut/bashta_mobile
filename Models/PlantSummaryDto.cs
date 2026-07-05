using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class PlantSummaryDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("plantTypeName")]
    public string? PlantTypeName { get; set; }

    [JsonPropertyName("plantedAt")]
    public DateTime? PlantedAt { get; set; }
}
