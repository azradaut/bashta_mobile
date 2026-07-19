using System.Text.Json.Serialization;

namespace bashta_mobile.Models;

public class PlantPhotoResponseDto
{
    [JsonPropertyName("plantId")]
    public int PlantId { get; set; }

    [JsonPropertyName("imagePath")]
    public string? ImagePath { get; set; }
}