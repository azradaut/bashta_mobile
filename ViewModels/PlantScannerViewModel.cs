using System.Collections.ObjectModel;
using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class PlantScannerViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private FileResult? _selectedImage;
    private ImageSource? _previewImage;
    private DiseaseDetectionResponse? _result;

    private ScannerPotOptionViewModel? _selectedPot;
    private int? _selectedPlantId;

    private string _selectedPlantName = "Nije odabrana biljka";
    private string _statusMessage = "Odaberi saksiju i fotografiju lista za analizu.";
    private bool _isLoadingPots;

    public PlantScannerViewModel(IApiService apiService)
    {
        _apiService = apiService;

        Pots = new ObservableCollection<ScannerPotOptionViewModel>();

        LoadCommand = new Command(async () => await LoadAsync());
        PickImageCommand = new Command(async () => await PickImageAsync());
        AnalyzeCommand = new Command(async () => await AnalyzeAsync());
    }

    public ObservableCollection<ScannerPotOptionViewModel> Pots { get; }

    public ICommand LoadCommand { get; }
    public ICommand PickImageCommand { get; }
    public ICommand AnalyzeCommand { get; }

    public ScannerPotOptionViewModel? SelectedPot
    {
        get => _selectedPot;
        set
        {
            if (_selectedPot == value)
                return;

            SetProperty(ref _selectedPot, value);

            if (!_isLoadingPots)
                ApplySelectedPot();
        }
    }

    public ImageSource? PreviewImage
    {
        get => _previewImage;
        set => SetProperty(ref _previewImage, value);
    }

    public DiseaseDetectionResponse? Result
    {
        get => _result;
        set
        {
            SetProperty(ref _result, value);
            OnPropertyChanged(nameof(HasResult));
            OnPropertyChanged(nameof(ResultTitle));
            OnPropertyChanged(nameof(ResultConfidence));
            OnPropertyChanged(nameof(ResultRecommendation));
        }
    }

    public bool HasResult => Result is not null;

    public string SelectedPlantName
    {
        get => _selectedPlantName;
        set => SetProperty(ref _selectedPlantName, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ResultTitle =>
        Result is null
            ? string.Empty
            : Result.IsHealthy
                ? "Biljka izgleda zdravo"
                : $"Detektovana bolest: {Result.DiseaseNameLocal ?? Result.DiseaseName}";

    public string ResultConfidence =>
        Result is null
            ? string.Empty
            : $"Pouzdanost modela: {Result.Confidence:P1}";

    public string ResultRecommendation =>
        Result?.TreatmentRecommendation ?? string.Empty;

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            _isLoadingPots = true;

            var previouslySelectedPotId = SelectedPot?.Id;

            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);

            Pots.Clear();

            foreach (var pot in pots)
            {
                var activePlant = pot.Plants.FirstOrDefault();

                Pots.Add(new ScannerPotOptionViewModel
                {
                    Id = pot.Id,
                    Name = pot.Name ?? $"Saksija #{pot.Id}",
                    Location = pot.Location,
                    DisplayName = string.IsNullOrWhiteSpace(pot.Location)
                        ? pot.Name ?? $"Saksija #{pot.Id}"
                        : $"{pot.Name} - {pot.Location}",

                    PlantId = activePlant?.Id,
                    PlantName = activePlant?.Nickname
                        ?? activePlant?.PlantTypeName
                        ?? "Nema aktivne biljke"
                });
            }

            if (Pots.Count == 0)
            {
                ClearSelection("Nije pronađena nijedna saksija.");
                return;
            }

            SelectedPot =
                Pots.FirstOrDefault(p => p.Id == previouslySelectedPotId)
                ?? Pots.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju saksija: {ex.Message}";
            ClearSelection("Nije moguće učitati saksije.");
        }
        finally
        {
            _isLoadingPots = false;
            IsBusy = false;
        }

        ApplySelectedPot();
    }

    private void ApplySelectedPot()
    {
        Result = null;

        if (SelectedPot is null)
        {
            ClearSelection("Odaberi saksiju za analizu.");
            return;
        }

        _selectedPlantId = SelectedPot.PlantId;
        SelectedPlantName = SelectedPot.PlantName;

        if (_selectedPlantId is null)
        {
            StatusMessage = "Odabrana saksija nema aktivnu biljku. Dodaj biljku prije analize.";
            return;
        }

        StatusMessage = $"Analiza će biti vezana za biljku: {SelectedPlantName}.";
    }

    private async Task PickImageAsync()
    {
        try
        {
            ErrorMessage = null;

            var image = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Odaberi fotografiju lista"
            });

            if (image is null)
                return;

            _selectedImage = image;

            var stream = await image.OpenReadAsync();
            PreviewImage = ImageSource.FromStream(() => stream);

            Result = null;
            StatusMessage = "Fotografija je odabrana. Pokreni analizu.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri odabiru slike: {ex.Message}";
        }
    }

    private async Task AnalyzeAsync()
    {
        if (IsBusy)
            return;

        if (SelectedPot is null)
        {
            ErrorMessage = "Prvo odaberi saksiju.";
            return;
        }

        if (_selectedPlantId is null)
        {
            ErrorMessage = "Odabrana saksija nema aktivnu biljku.";
            return;
        }

        if (_selectedImage is null)
        {
            ErrorMessage = "Prvo odaberi fotografiju lista.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            StatusMessage = "Analiza slike je u toku...";

            Result = await _apiService.AnalyzeDiseaseAsync(
                _selectedPlantId.Value,
                _selectedImage);

            StatusMessage = "Analiza je završena.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri analizi slike: {ex.Message}";
            StatusMessage = "Analiza nije uspjela.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearSelection(string message)
    {
        _selectedPlantId = null;
        SelectedPlantName = "Nije odabrana biljka";
        StatusMessage = message;
        Result = null;
    }
}

public class ScannerPotOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public int? PlantId { get; set; }

    public string PlantName { get; set; } = "Nema aktivne biljke";
}