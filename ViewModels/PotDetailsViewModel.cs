using System.Windows.Input;
using bashta_mobile.Helpers;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class PotDetailsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private PlantPotCardViewModel? _pot;

    private string _potName = string.Empty;
    private string _location = string.Empty;
    private string _plantName = string.Empty;
    private string _plantTypeName = string.Empty;
    private string _plantImageSource = "plant_default.png";

    private string _currentMoisture = "--";
    private string _idealMoisture = "--";
    private string _currentTemperature = "--";
    private string _idealTemperature = "--";
    private string _humidity = "--";
    private string _light = "--";
    private string _lastSensorReading = "--";

    private string _wateringInfo = "--";
    private string _wateringCount = "--";

    private string _healthStatus = "Nema prethodnih analiza bolesti.";
    private string _healthDate = string.Empty;
    private string _healthRecommendation = string.Empty;

    private string _overallStatus = "Učitavanje detalja biljke...";

    public PotDetailsViewModel(IApiService apiService)
    {
        _apiService = apiService;
        RefreshCommand = new Command(async () => await LoadAsync());
    }

    public ICommand RefreshCommand { get; }

    public string PotName
    {
        get => _potName;
        set => SetProperty(ref _potName, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public string PlantName
    {
        get => _plantName;
        set => SetProperty(ref _plantName, value);
    }

    public string PlantTypeName
    {
        get => _plantTypeName;
        set => SetProperty(ref _plantTypeName, value);
    }

    public string PlantImageSource
    {
        get => _plantImageSource;
        set => SetProperty(ref _plantImageSource, value);
    }

    public string CurrentMoisture
    {
        get => _currentMoisture;
        set => SetProperty(ref _currentMoisture, value);
    }

    public string IdealMoisture
    {
        get => _idealMoisture;
        set => SetProperty(ref _idealMoisture, value);
    }

    public string CurrentTemperature
    {
        get => _currentTemperature;
        set => SetProperty(ref _currentTemperature, value);
    }

    public string IdealTemperature
    {
        get => _idealTemperature;
        set => SetProperty(ref _idealTemperature, value);
    }

    public string Humidity
    {
        get => _humidity;
        set => SetProperty(ref _humidity, value);
    }

    public string Light
    {
        get => _light;
        set => SetProperty(ref _light, value);
    }

    public string LastSensorReading
    {
        get => _lastSensorReading;
        set => SetProperty(ref _lastSensorReading, value);
    }

    public string WateringInfo
    {
        get => _wateringInfo;
        set => SetProperty(ref _wateringInfo, value);
    }

    public string WateringCount
    {
        get => _wateringCount;
        set => SetProperty(ref _wateringCount, value);
    }

    public string HealthStatus
    {
        get => _healthStatus;
        set => SetProperty(ref _healthStatus, value);
    }

    public string HealthDate
    {
        get => _healthDate;
        set => SetProperty(ref _healthDate, value);
    }

    public string HealthRecommendation
    {
        get => _healthRecommendation;
        set => SetProperty(ref _healthRecommendation, value);
    }

    public string OverallStatus
    {
        get => _overallStatus;
        set => SetProperty(ref _overallStatus, value);
    }

    public async Task LoadFromPotAsync(PlantPotCardViewModel pot)
    {
        _pot = pot;

        PotName = pot.Name;
        Location = string.IsNullOrWhiteSpace(pot.RawLocation)
            ? "Lokacija nije unesena"
            : pot.RawLocation;

        PlantName = pot.PlantName;
        PlantTypeName = pot.PlantTypeName;
        PlantImageSource = PlantImageHelper.GetDefaultImage(pot.PlantTypeName);

        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        if (_pot is null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var sensor = await _apiService.GetLatestSensorReadingAsync(_pot.Id);
            var wateringStatus = await _apiService.GetWateringStatusAsync(_pot.Id);
            var activePlant = await _apiService.GetActivePlantByPotAsync(_pot.Id);

            if (activePlant is not null)
            {
                PlantName = activePlant.Nickname
                    ?? activePlant.PlantType?.DisplayName
                    ?? PlantName;

                PlantTypeName = activePlant.PlantType?.DisplayName
                    ?? PlantTypeName;

                PlantImageSource = PlantImageHelper.GetDefaultImage(PlantTypeName);

                IdealTemperature =
                    activePlant.PlantType?.MinTemp is null ||
                    activePlant.PlantType?.MaxTemp is null
                        ? "--"
                        : $"{activePlant.PlantType.MinTemp:0.#}–{activePlant.PlantType.MaxTemp:0.#} °C";
            }

            CurrentMoisture = sensor?.SoilMoisture is null
                ? "--"
                : $"{sensor.SoilMoisture:0.#}%";

            CurrentTemperature = sensor?.Temperature is null
                ? "--"
                : $"{sensor.Temperature:0.#} °C";

            Humidity = sensor?.Humidity is null
                ? "--"
                : $"{sensor.Humidity:0.#}%";

            Light = sensor?.Lux is null
                ? "--"
                : $"{sensor.Lux:0.#} lux";

            LastSensorReading = sensor is null
                ? "Nema senzorskih očitanja."
                : $"Zadnje očitanje: {sensor.Time.ToLocalTime():dd.MM.yyyy HH:mm}";

            IdealMoisture =
                wateringStatus?.MinRecommendedSoilMoisture is null ||
                wateringStatus?.MaxRecommendedSoilMoisture is null
                    ? "--"
                    : $"{wateringStatus.MinRecommendedSoilMoisture}–{wateringStatus.MaxRecommendedSoilMoisture}%";

            WateringInfo = wateringStatus?.LastWateredAt is null
                ? "Nema evidentiranog zalijevanja."
                : $"Zadnje zalijevanje: {wateringStatus.LastWateredAt.Value.ToLocalTime():dd.MM.yyyy HH:mm}";

            WateringCount = wateringStatus is null
                ? "--"
                : $"Zalijevanja u posljednja 24h: {wateringStatus.WateringCountLast24h}/{wateringStatus.MaxWateringCountLast24h}";

            OverallStatus = wateringStatus?.StatusMessage
                ?? "Nema dostupne sistemske preporuke.";

            if (_pot.PlantId is not null)
            {
                var latestDisease = await _apiService.GetLatestDiseaseDetectionAsync(_pot.PlantId.Value);

                if (latestDisease is null)
                {
                    HealthStatus = "Nema prethodnih analiza bolesti.";
                    HealthDate = string.Empty;
                    HealthRecommendation = string.Empty;
                }
                else
                {
                    HealthStatus = latestDisease.IsHealthy
                        ? "Posljednja analiza: biljka izgleda zdravo."
                        : $"Posljednja analiza: {latestDisease.DiseaseNameLocal ?? latestDisease.DiseaseName}";

                    HealthDate =
                        $"Datum detekcije: {latestDisease.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}";

                    HealthRecommendation =
                        latestDisease.TreatmentRecommendation ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju detalja biljke: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}