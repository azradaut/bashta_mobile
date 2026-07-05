using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Models;
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
    }

    public ObservableCollection<PlantPotCardViewModel> Pots { get; }

    public ICommand LoadCommand { get; }
    public ICommand AddPotCommand { get; }

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

            // MVP: jedan korisnik, ID = 1
            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);

            foreach (var pot in pots)
            {
                var activePlant = pot.Plants.FirstOrDefault();

                Pots.Add(new PlantPotCardViewModel
                {
                    Id = pot.Id,
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

                    StatusText = pot.IsActive ? "Aktivna saksija" : "Neaktivna saksija",
                    CreatedAtText = $"Dodana: {pot.CreatedAt.ToLocalTime():dd.MM.yyyy}"
                });
            }

            HasPots = Pots.Count > 0;

            SummaryText = HasPots
                ? $"Ukupno saksija: {Pots.Count}"
                : "Trenutno nema dodanih saksija.";
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

    private async Task AddPotAsync()
    {
        await Shell.Current.DisplayAlert(
            "Dodavanje saksije",
            "U MVP verziji prikazuje se jedna testna saksija. Funkcionalnost dodavanja više saksija predviđena je za narednu fazu razvoja.",
            "U redu"
        );
    }
}

public class PlantPotCardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PlantName { get; set; } = string.Empty;
    public string PlantTypeName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string CreatedAtText { get; set; } = string.Empty;
}