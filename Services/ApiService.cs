using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using bashta_mobile.Models;

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
}
