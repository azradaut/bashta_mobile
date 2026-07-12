using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class WateringViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private PlantPotOptionViewModel? _selectedPot;
    private bool _isLoadingPots;

    private string _plantName = "Nema aktivne biljke";
    private string _currentMoisture = "--";
    private string _recommendedMoisture = "--";
    private string _lastWatered = "Nema evidentiranog zalijevanja";
    private string _wateringCount = "--";
    private string _statusMessage = "Odaberi saksiju za prikaz statusa zalijevanja.";
    private string? _warningMessage;
    private bool _canWater;
    private int _recommendedAmountMl = 200;

    public WateringViewModel(IApiService apiService)
    {
        _apiService = apiService;

        Pots = new ObservableCollection<PlantPotOptionViewModel>();
        RecentEvents = new ObservableCollection<WateringHistoryItemViewModel>();

        LoadCommand = new Command(async () => await LoadAsync());
        ManualWaterCommand = new Command(async () => await ManualWaterAsync());
    }

    public ObservableCollection<PlantPotOptionViewModel> Pots { get; }

    public ObservableCollection<WateringHistoryItemViewModel> RecentEvents { get; }

    public ICommand LoadCommand { get; }

    public ICommand ManualWaterCommand { get; }

    public PlantPotOptionViewModel? SelectedPot
    {
        get => _selectedPot;
        set
        {
            if (_selectedPot == value)
                return;

            SetProperty(ref _selectedPot, value);

            if (!_isLoadingPots && value is not null)
            {
                _ = LoadStatusForSelectedPotAsync();
            }
        }
    }

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
            _isLoadingPots = true;

            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);

            var previouslySelectedPotId = SelectedPot?.Id;

            Pots.Clear();

            foreach (var pot in pots)
            {
                Pots.Add(new PlantPotOptionViewModel
                {
                    Id = pot.Id,
                    Name = pot.Name ?? $"Saksija #{pot.Id}",
                    Location = pot.Location,
                    DisplayName = string.IsNullOrWhiteSpace(pot.Location)
                        ? pot.Name ?? $"Saksija #{pot.Id}"
                        : $"{pot.Name} - {pot.Location}"
                });
            }

            if (Pots.Count == 0)
            {
                ClearStatus("Nije pronađena nijedna saksija.");
                return;
            }

            SelectedPot =
                Pots.FirstOrDefault(p => p.Id == previouslySelectedPotId)
                ?? Pots.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju saksija: {ex.Message}";
            ClearStatus("Nije moguće učitati saksije.");
        }
        finally
        {
            _isLoadingPots = false;
            IsBusy = false;
        }

        await LoadStatusForSelectedPotAsync();
    }

    private async Task LoadStatusForSelectedPotAsync()
    {
        if (SelectedPot is null)
        {
            ClearStatus("Odaberi saksiju za prikaz statusa zalijevanja.");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            WarningMessage = null;

            var status = await _apiService.GetWateringStatusAsync(SelectedPot.Id);

            if (status is null)
            {
                ClearStatus("Nije moguće učitati status zalijevanja.");
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

        if (SelectedPot is null)
        {
            ErrorMessage = "Odaberi saksiju koju želiš zaliti.";
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Ručno zalijevanje",
            $"Evidentirati zalijevanje saksije \"{SelectedPot.Name}\" sa {_recommendedAmountMl} ml?",
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
                PotId = SelectedPot.Id,
                DurationSec = 10,
                AmountMl = _recommendedAmountMl
            });

            await Shell.Current.DisplayAlert(
                "Uspješno",
                "Zalijevanje je evidentirano.",
                "U redu");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri zalijevanju: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }

        await LoadStatusForSelectedPotAsync();
    }

    private void ClearStatus(string message)
    {
        PlantName = "Nema aktivne biljke";
        CurrentMoisture = "--";
        RecommendedMoisture = "--";
        LastWatered = "Nema evidentiranog zalijevanja";
        WateringCount = "--";
        StatusMessage = message;
        WarningMessage = null;
        CanWater = false;
        RecentEvents.Clear();
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

public class PlantPotOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string DisplayName { get; set; } = string.Empty;
}

public class WateringHistoryItemViewModel
{
    public string Title { get; set; } = string.Empty;

    public string Details { get; set; } = string.Empty;

    public string CreatedAtText { get; set; } = string.Empty;
}