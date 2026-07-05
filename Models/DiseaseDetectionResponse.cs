using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class DiseaseDetectionResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("plantId")]
    public int PlantId { get; set; }

    [JsonPropertyName("diseaseName")]
    public string? DiseaseName { get; set; }

    [JsonPropertyName("diseaseNameLocal")]
    public string? DiseaseNameLocal { get; set; }

    [JsonPropertyName("confidence")]
    public decimal? Confidence { get; set; }

    [JsonPropertyName("isHealthy")]
    public bool IsHealthy { get; set; }

    [JsonPropertyName("treatmentRecommendation")]
    public string? TreatmentRecommendation { get; set; }

    [JsonPropertyName("imagePath")]
    public string? ImagePath { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}