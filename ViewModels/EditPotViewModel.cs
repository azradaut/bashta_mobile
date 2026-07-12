using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class EditPotViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private int _plantPotId;
    private int? _currentPlantId;
    private int? _currentPlantTypeId;

    private string _name = string.Empty;
    private string _location = string.Empty;
    private string _macAddress = string.Empty;
    private string _firmwareVersion = string.Empty;
    private string _plantNickname = string.Empty;
    private string _statusMessage = "Uredi podatke o pametnoj saksiji.";
    private PlantTypeDto? _selectedPlantType;

    public EditPotViewModel(IApiService apiService)
    {
        _apiService = apiService;

        PlantTypes = new ObservableCollection<PlantTypeDto>();

        SaveCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await CancelAsync());
    }

    public ObservableCollection<PlantTypeDto> PlantTypes { get; }

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

    public async Task LoadFromPotAsync(PlantPotCardViewModel pot)
    {
        _plantPotId = pot.Id;
        _currentPlantId = pot.PlantId;
        _currentPlantTypeId = pot.PlantTypeId;

        Name = pot.Name;
        Location = pot.RawLocation;
        MacAddress = pot.MacAddress;
        FirmwareVersion = string.IsNullOrWhiteSpace(pot.FirmwareVersion)
            ? "1.0.0"
            : pot.FirmwareVersion;

        PlantNickname = pot.PlantNickname;

        StatusMessage = $"Uređuješ saksiju: {pot.Name}";
        ErrorMessage = null;

        await LoadPlantTypesAsync();

        SelectedPlantType = PlantTypes.FirstOrDefault(x => x.Id == _currentPlantTypeId)
            ?? PlantTypes.FirstOrDefault();
    }

    private async Task LoadPlantTypesAsync()
    {
        if (PlantTypes.Count > 0)
            return;

        var plantTypes = await _apiService.GetPlantTypesAsync();

        PlantTypes.Clear();

        foreach (var plantType in plantTypes)
            PlantTypes.Add(plantType);
    }

    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (_plantPotId <= 0)
        {
            ErrorMessage = "Nije pronađen ID saksije za uređivanje.";
            return;
        }

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
            StatusMessage = "Promjene se spremaju...";

            var potRequest = new CreatePlantPotRequest
            {
                Name = Name.Trim(),
                Location = string.IsNullOrWhiteSpace(Location) ? null : Location.Trim(),
                MacAddress = string.IsNullOrWhiteSpace(MacAddress) ? null : MacAddress.Trim(),
                FirmwareVersion = string.IsNullOrWhiteSpace(FirmwareVersion) ? null : FirmwareVersion.Trim()
            };

            await _apiService.UpdatePlantPotAsync(_plantPotId, potRequest);

            var selectedPlantTypeId = SelectedPlantType.Id;
            var plantTypeChanged = _currentPlantTypeId != selectedPlantTypeId;

            if (_currentPlantId is null)
            {
                await CreateNewPlantAsync(selectedPlantTypeId);
            }
            else if (plantTypeChanged)
            {
                await _apiService.RemovePlantAsync(_currentPlantId.Value);
                await CreateNewPlantAsync(selectedPlantTypeId);
            }

            await Shell.Current.DisplayAlert(
                "Uspješno",
                "Podaci o saksiji su ažurirani.",
                "U redu");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri ažuriranju saksije: {ex.Message}";
            StatusMessage = "Ažuriranje nije uspjelo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateNewPlantAsync(int plantTypeId)
    {
        await _apiService.CreatePlantAsync(new CreatePlantRequest
        {
            PotId = _plantPotId,
            PlantTypeId = plantTypeId,
            Nickname = string.IsNullOrWhiteSpace(PlantNickname)
                ? SelectedPlantType?.DisplayName
                : PlantNickname.Trim(),
            PlantedAt = DateOnly.FromDateTime(DateTime.Today),
            Notes = "Ažurirano kroz mobilnu aplikaciju."
        });
    }

    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}