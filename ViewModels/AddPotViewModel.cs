using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class AddPotViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private string _name = string.Empty;
    private string _location = string.Empty;
    private string _macAddress = string.Empty;
    private string _firmwareVersion = "1.0.0";
    private string _plantNickname = string.Empty;
    private string _statusMessage = "Unesi osnovne podatke o pametnoj saksiji.";
    private PlantTypeDto? _selectedPlantType;

    public AddPotViewModel(IApiService apiService)
    {
        _apiService = apiService;

        PlantTypes = new ObservableCollection<PlantTypeDto>();

        LoadCommand = new Command(async () => await LoadAsync());
        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    public ObservableCollection<PlantTypeDto> PlantTypes { get; }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public string MacAddress
    {
        get => _macAddress;
        set => SetProperty(ref _macAddress, value);
    }

    public string FirmwareVersion
    {
        get => _firmwareVersion;
        set => SetProperty(ref _firmwareVersion, value);
    }

    public string PlantNickname
    {
        get => _plantNickname;
        set => SetProperty(ref _plantNickname, value);
    }

    public PlantTypeDto? SelectedPlantType
    {
        get => _selectedPlantType;
        set => SetProperty(ref _selectedPlantType, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = null;

            if (PlantTypes.Count > 0)
                return;

            var plantTypes = await _apiService.GetPlantTypesAsync();

            PlantTypes.Clear();

            foreach (var plantType in plantTypes)
                PlantTypes.Add(plantType);

            SelectedPlantType = PlantTypes.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju dostupnih biljaka: {ex.Message}";
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Naziv saksije je obavezan.";
            return;
        }

        if (SelectedPlantType is null)
        {
            ErrorMessage = "Odaberi biljku koja je posađena u saksiji.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            StatusMessage = "Saksija se dodaje...";

            var potRequest = new CreatePlantPotRequest
            {
                Name = Name.Trim(),
                Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
                MacAddress = string.IsNullOrWhiteSpace(MacAddress) ? null : MacAddress.Trim(),
                FirmwareVersion = string.IsNullOrWhiteSpace(FirmwareVersion) ? null : FirmwareVersion.Trim()
            };

            var createdPot = await _apiService.CreatePlantPotAsync(userId: 1, potRequest);

            if (createdPot is null)
            {
                ErrorMessage = "Saksija je kreirana, ali aplikacija nije dobila njen ID.";
                return;
            }

            var plantRequest = new CreatePlantRequest
            {
                PotId = createdPot.Id,
                PlantTypeId = SelectedPlantType.Id,
                Nickname = string.IsNullOrWhiteSpace(PlantNickname)
                    ? SelectedPlantType.DisplayName
                    : PlantNickname.Trim(),
                PlantedAt = DateOnly.FromDateTime(DateTime.Today),
                Notes = "Dodano kroz mobilnu aplikaciju."
            };

            await _apiService.CreatePlantAsync(plantRequest);

            await Shell.Current.DisplayAlert(
                "Uspješno",
                "Nova saksija i biljka su dodane.",
                "U redu");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri dodavanju saksije: {ex.Message}";
            StatusMessage = "Dodavanje nije uspjelo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}