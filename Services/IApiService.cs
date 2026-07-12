using bashta_mobile.Models;

namespace bashta_mobile.Services;

public interface IApiService
{
    Task<List<PlantPotDto>> GetPlantPotsByUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<SensorReadingDto?> GetLatestSensorReadingAsync(
        int plantPotId,
        CancellationToken cancellationToken = default);

    Task<List<RecommendationDto>> GetRecommendationsAsync(
        int plantId,
        CancellationToken cancellationToken = default);

    Task<DiseaseDetectionResponse?> GetLatestDiseaseDetectionAsync(
        int plantId,
        CancellationToken cancellationToken = default);

    Task<DiseaseDetectionResponse?> AnalyzeDiseaseAsync(
        int plantId,
        FileResult imageFile,
        CancellationToken cancellationToken = default);

    Task<PlantPotDto?> CreatePlantPotAsync(
        int userId,
        CreatePlantPotRequest request,
        CancellationToken cancellationToken = default);

    Task UpdatePlantPotAsync(
        int plantPotId,
        CreatePlantPotRequest request,
        CancellationToken cancellationToken = default);

    Task DeletePlantPotAsync(
        int plantPotId,
        CancellationToken cancellationToken = default);

    Task<List<PlantTypeDto>> GetPlantTypesAsync(
        CancellationToken cancellationToken = default);

    Task CreatePlantAsync(
        CreatePlantRequest request,
        CancellationToken cancellationToken = default);

    Task RemovePlantAsync(
        int plantId,
        CancellationToken cancellationToken = default);

    Task<WateringStatusDto?> GetWateringStatusAsync(
    int plantPotId,
    CancellationToken cancellationToken = default);

    Task<WateringEventDto?> ManualWaterAsync(
        ManualWateringRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PlantDetailsDto?> GetActivePlantByPotAsync(
    int potId,
    CancellationToken cancellationToken = default);
}