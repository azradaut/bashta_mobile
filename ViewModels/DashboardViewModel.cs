using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private string _potName = "Bashta Pot";
    private string _plantName = "Paradajz";
    private string _soilMoisture = "--";
    private string _temperature = "--";
    private string _airHumidity = "--";
    private string _lightIntensity = "--";
    private string _lastReading = "Nema očitanja";
    private string _recommendation = "Učitavanje preporuke...";
    private string _diseaseStatus = "Nema posljednje analize lista.";

    public DashboardViewModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadCommand = new Command(async () => await LoadAsync());
    }

    public ICommand LoadCommand { get; }

    public string PotName
    {
        get => _potName;
        set => SetProperty(ref _potName, value);
    }

    public string PlantName
    {
        get => _plantName;
        set => SetProperty(ref _plantName, value);
    }

    public string SoilMoisture
    {
        get => _soilMoisture;
        set => SetProperty(ref _soilMoisture, value);
    }

    public string Temperature
    {
        get => _temperature;
        set => SetProperty(ref _temperature, value);
    }

    public string AirHumidity
    {
        get => _airHumidity;
        set => SetProperty(ref _airHumidity, value);
    }

    public string LightIntensity
    {
        get => _lightIntensity;
        set => SetProperty(ref _lightIntensity, value);
    }

    public string LastReading
    {
        get => _lastReading;
        set => SetProperty(ref _lastReading, value);
    }

    public string Recommendation
    {
        get => _recommendation;
        set => SetProperty(ref _recommendation, value);
    }

    public string DiseaseStatus
    {
        get => _diseaseStatus;
        set => SetProperty(ref _diseaseStatus, value);
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // MVP: jedan korisnik, ID = 1
            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);
            var pot = pots.FirstOrDefault();

            if (pot is null)
            {
                ErrorMessage = "Nije pronađena nijedna saksija za korisnika.";
                return;
            }

            var activePlant = pot.Plants.FirstOrDefault();

            PotName = string.IsNullOrWhiteSpace(pot.Name)
                ? $"Saksija #{pot.Id}"
                : pot.Name;

            PlantName = activePlant is null
                ? "Nema aktivne biljke"
                : !string.IsNullOrWhiteSpace(activePlant.Nickname)
                    ? activePlant.Nickname
                    : activePlant.PlantTypeName ?? "Paradajz";

            var reading = await _apiService.GetLatestSensorReadingAsync(pot.Id);

            if (reading is not null)
            {
                SoilMoisture = FormatPercentage(reading.SoilMoisture);
                Temperature = FormatTemperature(reading.Temperature);
                AirHumidity = FormatPercentage(reading.Humidity);
                LightIntensity = FormatLight(reading.Lux);
                LastReading = $"Zadnje očitanje: {reading.Time.ToLocalTime():dd.MM.yyyy HH:mm}";
            }
            else
            {
                SoilMoisture = "--";
                Temperature = "--";
                AirHumidity = "--";
                LightIntensity = "--";
                LastReading = "Nema očitanja";
            }

            if (activePlant is not null)
            {
                var recommendations = await _apiService.GetRecommendationsAsync(activePlant.Id);

                var latestRecommendation = recommendations
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault();

                Recommendation = latestRecommendation?.Message
                    ?? "Trenutno nema posebnih preporuka.";

                var disease = await _apiService.GetLatestDiseaseDetectionAsync(activePlant.Id);

                if (disease is not null)
                {
                    DiseaseStatus = disease.IsHealthy
                        ? $"Zadnja analiza: biljka izgleda zdravo ({FormatConfidence(disease.Confidence)})."
                        : $"Zadnja analiza: moguća bolest - {disease.DiseaseNameLocal ?? disease.DiseaseName} ({FormatConfidence(disease.Confidence)}).";
                }
                else
                {
                    DiseaseStatus = "Nema posljednje analize lista.";
                }
            }
            else
            {
                Recommendation = "Dodaj biljku u saksiju kako bi sistem mogao prikazati preporuke.";
                DiseaseStatus = "Nema biljke za analizu.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju podataka: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string FormatPercentage(decimal? value)
    {
        return value is null ? "--" : $"{value:0.#}%";
    }

    private static string FormatTemperature(decimal? value)
    {
        return value is null ? "--" : $"{value:0.#} °C";
    }

    private static string FormatLight(decimal? value)
    {
        return value is null ? "--" : $"{value:0.#} lux";
    }

    private static string FormatConfidence(decimal? value)
    {
        if (value is null)
            return "bez podatka o pouzdanosti";

        return $"{value * 100:0.#}%";
    }
}