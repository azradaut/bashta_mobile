using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using bashta_mobile.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace bashta_mobile.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PlantPotDto>> GetPlantPotsByUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<PlantPotDto>>(
            $"plantpot/user/{userId}",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<SensorReadingDto?> GetLatestSensorReadingAsync(
        int plantPotId,
        CancellationToken cancellationToken = default)
    {
        return await GetOrNullAsync<SensorReadingDto>(
            $"sensor/{plantPotId}/latest",
            cancellationToken);
    }

    public async Task<List<RecommendationDto>> GetRecommendationsAsync(
        int plantId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<RecommendationDto>>(
            $"recommendation/{plantId}",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task<DiseaseDetectionResponse?> GetLatestDiseaseDetectionAsync(
        int plantId,
        CancellationToken cancellationToken = default)
    {
        return await GetOrNullAsync<DiseaseDetectionResponse>(
            $"disease/latest/{plantId}",
            cancellationToken);
    }

    private async Task<T?> GetOrNullAsync<T>(
        string url,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<T>(
            JsonOptions,
            cancellationToken);
    }
    public async Task<DiseaseDetectionResponse?> AnalyzeDiseaseAsync(
    int plantId,
    FileResult imageFile,
    CancellationToken cancellationToken = default)
    {
        await using var imageStream = await imageFile.OpenReadAsync();

        using var formData = new MultipartFormDataContent();

        formData.Add(
            new StringContent(plantId.ToString()),
            "PlantId"
        );

        var imageContent = new StreamContent(imageStream);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(imageFile.FileName));

        formData.Add(
            imageContent,
            "Image",
            imageFile.FileName
        );

        var response = await _httpClient.PostAsync(
            "disease/analyze",
            formData,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DiseaseDetectionResponse>(
            JsonOptions,
            cancellationToken
        );
    }
    public async Task<PlantPotDto?> CreatePlantPotAsync(
    int userId,
    CreatePlantPotRequest request,
    CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"plantpot?userId={userId}",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PlantPotDto>(
            JsonOptions,
            cancellationToken);
    }

    public async Task UpdatePlantPotAsync(
    int plantPotId,
    CreatePlantPotRequest request,
    CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"plantpot/{plantPotId}",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePlantPotAsync(
        int plantPotId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(
            $"plantpot/{plantPotId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<List<PlantTypeDto>> GetPlantTypesAsync(
    CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<PlantTypeDto>>(
            "plant/types",
            JsonOptions,
            cancellationToken);

        return result ?? [];
    }

    public async Task CreatePlantAsync(
        CreatePlantRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "plant",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task RemovePlantAsync(
        int plantId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PatchAsync(
            $"plant/{plantId}/remove",
            content: null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task<WateringStatusDto?> GetWateringStatusAsync(
    int plantPotId,
    CancellationToken cancellationToken = default)
    {
        return await GetOrNullAsync<WateringStatusDto>(
            $"watering/{plantPotId}/status",
            cancellationToken);
    }

    public async Task<WateringEventDto?> ManualWaterAsync(
        ManualWateringRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "watering/manual",
            request,
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WateringEventDto>(
            JsonOptions,
            cancellationToken);
    }
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
