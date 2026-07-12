using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class MyPotsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private bool _hasPots;
    private string _summaryText = "Učitavanje saksija...";

    public MyPotsViewModel(IApiService apiService)
    {
        _apiService = apiService;

        Pots = new ObservableCollection<PlantPotCardViewModel>();

        LoadCommand = new Command(async () => await LoadAsync());
        AddPotCommand = new Command(async () => await AddPotAsync());
        EditPotCommand = new Command<PlantPotCardViewModel>(async pot => await EditPotAsync(pot));
        DeletePotCommand = new Command<PlantPotCardViewModel>(async pot => await DeletePotAsync(pot));
    }

    public ObservableCollection<PlantPotCardViewModel> Pots { get; }

    public ICommand LoadCommand { get; }
    public ICommand AddPotCommand { get; }
    public ICommand EditPotCommand { get; }
    public ICommand DeletePotCommand { get; }

    public bool HasPots
    {
        get => _hasPots;
        set => SetProperty(ref _hasPots, value);
    }

    public string SummaryText
    {
        get => _summaryText;
        set => SetProperty(ref _summaryText, value);
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            Pots.Clear();

            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);

            foreach (var pot in pots)
            {
                var activePlant = pot.Plants.FirstOrDefault();

                Pots.Add(new PlantPotCardViewModel
                {
                    Id = pot.Id,
                    PlantId = activePlant?.Id,
                    PlantTypeId = activePlant?.PlantTypeId,
                    PlantNickname = activePlant?.Nickname ?? string.Empty,

                    Name = string.IsNullOrWhiteSpace(pot.Name)
                        ? $"Saksija #{pot.Id}"
                        : pot.Name,

                    PlantName = activePlant is null
                        ? "Nema dodane biljke"
                        : !string.IsNullOrWhiteSpace(activePlant.Nickname)
                            ? activePlant.Nickname
                            : activePlant.PlantTypeName ?? "Paradajz",

                    PlantTypeName = activePlant?.PlantTypeName ?? "Nije definisano",

                    Location = string.IsNullOrWhiteSpace(pot.Location)
                        ? "Lokacija nije unesena"
                        : pot.Location,

                    RawLocation = pot.Location ?? string.Empty,
                    MacAddress = pot.MacAddress ?? string.Empty,
                    FirmwareVersion = pot.FirmwareVersion ?? string.Empty,

                    StatusText = pot.IsActive ? "Aktivna saksija" : "Neaktivna saksija",
                    CreatedAtText = $"Dodana: {pot.CreatedAt.ToLocalTime():dd.MM.yyyy}"
                });
            }

            UpdateSummary();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju saksija: {ex.Message}";
            SummaryText = "Saksije nije moguće učitati.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task AddPotAsync()
    {
        await Shell.Current.GoToAsync("add-pot");
    }

    private static async Task EditPotAsync(PlantPotCardViewModel? pot)
    {
        if (pot is null)
            return;

        await Shell.Current.GoToAsync("edit-pot", new Dictionary<string, object>
        {
            ["Pot"] = pot
        });
    }

    private async Task DeletePotAsync(PlantPotCardViewModel? pot)
    {
        if (pot is null || IsBusy)
            return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Brisanje saksije",
            $"Da li sigurno želiš obrisati saksiju \"{pot.Name}\"?",
            "Obriši",
            "Odustani");

        if (!confirmed)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _apiService.DeletePlantPotAsync(pot.Id);

            Pots.Remove(pot);
            UpdateSummary();

            await Shell.Current.DisplayAlert(
                "Uspješno",
                "Saksija je obrisana.",
                "U redu");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri brisanju saksije: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateSummary()
    {
        HasPots = Pots.Count > 0;

        SummaryText = HasPots
            ? $"Ukupno saksija: {Pots.Count}"
            : "Trenutno nema dodanih saksija.";
    }
}

public class PlantPotCardViewModel
{
    public int Id { get; set; }
    public int? PlantId { get; set; }
    public int? PlantTypeId { get; set; }
    public string PlantNickname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
    public string RawLocation { get; set; } = string.Empty;

    public string MacAddress { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;

    public string StatusText { get; set; } = string.Empty;
    public string CreatedAtText { get; set; } = string.Empty;
}