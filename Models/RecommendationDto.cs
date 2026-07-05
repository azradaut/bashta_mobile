using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class RecommendationDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("plantId")]
    public int PlantId { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}