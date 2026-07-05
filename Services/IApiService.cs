using System;
using System.Collections.Generic;
using System.Text;
using bashta_mobile.Models;

namespace bashta_mobile.Services;

public interface IApiService
{
    Task<List<PlantPotDto>> GetPlantPotsByUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<SensorReadingDto?> GetLatestSensorReadingAsync(int plantPotId, CancellationToken cancellationToken = default);

    Task<List<RecommendationDto>> GetRecommendationsAsync(int plantId, CancellationToken cancellationToken = default);

    Task<DiseaseDetectionResponse?> GetLatestDiseaseDetectionAsync(int plantId, CancellationToken cancellationToken = default);

    Task<DiseaseDetectionResponse?> AnalyzeDiseaseAsync(int plantId, FileResult imageFile, CancellationToken cancellationToken = default);
}
