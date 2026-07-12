using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class WateringViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private int? _activePotId;

    private string _plantName = "Nema aktivne biljke";
    private string _currentMoisture = "--";
    private string _recommendedMoisture = "--";
    private string _lastWatered = "Nema evidentiranog zalijevanja";
    private string _wateringCount = "--";
    private string _statusMessage = "Učitavanje statusa zalijevanja...";
    private string? _warningMessage;
    private bool _canWater;
    private int _recommendedAmountMl = 200;

    public WateringViewModel(IApiService apiService)
    {
        _apiService = apiService;

        RecentEvents = new ObservableCollection<WateringHistoryItemViewModel>();

        LoadCommand = new Command(async () => await LoadAsync());
        ManualWaterCommand = new Command(async () => await ManualWaterAsync());
    }

    public ObservableCollection<WateringHistoryItemViewModel> RecentEvents { get; }

    public ICommand LoadCommand { get; }
    public ICommand ManualWaterCommand { get; }

    public string PlantName
    {
        get => _plantName;
        set => SetProperty(ref _plantName, value);
    }

    public string CurrentMoisture
    {
        get => _currentMoisture;
        set => SetProperty(ref _currentMoisture, value);
    }

    public string RecommendedMoisture
    {
        get => _recommendedMoisture;
        set => SetProperty(ref _recommendedMoisture, value);
    }

    public string LastWatered
    {
        get => _lastWatered;
        set => SetProperty(ref _lastWatered, value);
    }

    public string WateringCount
    {
        get => _wateringCount;
        set => SetProperty(ref _wateringCount, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string? WarningMessage
    {
        get => _warningMessage;
        set => SetProperty(ref _warningMessage, value);
    }

    public bool CanWater
    {
        get => _canWater;
        set => SetProperty(ref _canWater, value);
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            WarningMessage = null;

            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);
            var pot = pots.FirstOrDefault();

            if (pot is null)
            {
                _activePotId = null;
                CanWater = false;
                StatusMessage = "Nije pronađena nijedna saksija.";
                return;
            }

            _activePotId = pot.Id;

            var status = await _apiService.GetWateringStatusAsync(pot.Id);

            if (status is null)
            {
                CanWater = false;
                StatusMessage = "Nije moguće učitati status zalijevanja.";
                return;
            }

            PlantName = status.PlantName ?? "Nema aktivne biljke";
            CurrentMoisture = status.CurrentSoilMoisture is null
                ? "--"
                : $"{status.CurrentSoilMoisture:0.#}%";

            RecommendedMoisture =
                status.MinRecommendedSoilMoisture is null ||
                status.MaxRecommendedSoilMoisture is null
                    ? "--"
                    : $"{status.MinRecommendedSoilMoisture}–{status.MaxRecommendedSoilMoisture}%";

            LastWatered = status.LastWateredAt is null
                ? "Nema evidentiranog zalijevanja"
                : $"Zadnje zalijevanje: {status.LastWateredAt.Value.ToLocalTime():dd.MM.yyyy HH:mm}";

            WateringCount =
                $"Zalijevanja u posljednja 24h: {status.WateringCountLast24h}/{status.MaxWateringCountLast24h}";

            StatusMessage = status.StatusMessage ?? "Nema dostupne preporuke.";
            WarningMessage = status.WarningMessage;

            _recommendedAmountMl = status.RecommendedAmountMl > 0
                ? status.RecommendedAmountMl
                : 200;

            CanWater = status.CanWater && status.RemainingWateringsLast24h > 0;

            RecentEvents.Clear();

            foreach (var item in status.RecentEvents)
            {
                RecentEvents.Add(new WateringHistoryItemViewModel
                {
                    Title = item.Skipped
                        ? "Preskočeno zalijevanje"
                        : item.TriggeredBy == "auto"
                            ? "Automatsko zalijevanje"
                            : "Ručno zalijevanje",

                    Details = BuildHistoryDetails(item),
                    CreatedAtText = item.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju zalijevanja: {ex.Message}";
            CanWater = false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ManualWaterAsync()
    {
        if (IsBusy)
            return;

        if (_activePotId is null)
        {
            ErrorMessage = "Nije pronađena aktivna saksija.";
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Ručno zalijevanje",
            $"Evidentirati zalijevanje od {_recommendedAmountMl} ml?",
            "Zalij",
            "Odustani");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _apiService.ManualWaterAsync(new ManualWateringRequestDto
            {
                PotId = _activePotId.Value,
                DurationSec = 10,
                AmountMl = _recommendedAmountMl
            });

            await Shell.Current.DisplayAlert(
                "Uspješno",
                "Zalijevanje je evidentirano.",
                "U redu");

            IsBusy = false;

            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri zalijevanju: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildHistoryDetails(WateringEventDto item)
    {
        if (item.Skipped)
            return item.SkipReason ?? "Zalijevanje je preskočeno.";

        var amount = item.AmountMl is null
            ? "količina nije evidentirana"
            : $"{item.AmountMl} ml";

        var moisture = item.SoilMoistureBefore is null
            ? "vlaga prije: nije dostupna"
            : $"vlaga prije: {item.SoilMoistureBefore}%";

        return $"{amount}, {moisture}";
    }
}

public class WateringHistoryItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string CreatedAtText { get; set; } = string.Empty;
}