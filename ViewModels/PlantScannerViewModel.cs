using System.Windows.Input;
using bashta_mobile.Models;
using bashta_mobile.Services;

namespace bashta_mobile.ViewModels;

public class PlantScannerViewModel : BaseViewModel
{
    private readonly IApiService _apiService;

    private FileResult? _selectedImageFile;
    private int? _activePlantId;

    private string _plantDisplayName = "Cherry paradajz";
    private string _statusMessage = "Odaberi ili uslikaj list paradajza za analizu.";
    private string _resultTitle = string.Empty;
    private string _confidenceText = string.Empty;
    private string _recommendation = string.Empty;
    private ImageSource? _selectedImageSource;
    private bool _hasSelectedImage;
    private bool _isResultVisible;

    public PlantScannerViewModel(IApiService apiService)
    {
        _apiService = apiService;

        LoadCommand = new Command(async () => await LoadAsync());
        PickImageCommand = new Command(async () => await PickImageAsync());
        TakePhotoCommand = new Command(async () => await TakePhotoAsync());
        AnalyzeCommand = new Command(async () => await AnalyzeAsync());
    }

    public ICommand LoadCommand { get; }
    public ICommand PickImageCommand { get; }
    public ICommand TakePhotoCommand { get; }
    public ICommand AnalyzeCommand { get; }

    public string PlantDisplayName
    {
        get => _plantDisplayName;
        set => SetProperty(ref _plantDisplayName, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ResultTitle
    {
        get => _resultTitle;
        set => SetProperty(ref _resultTitle, value);
    }

    public string ConfidenceText
    {
        get => _confidenceText;
        set => SetProperty(ref _confidenceText, value);
    }

    public string Recommendation
    {
        get => _recommendation;
        set => SetProperty(ref _recommendation, value);
    }

    public ImageSource? SelectedImageSource
    {
        get => _selectedImageSource;
        set => SetProperty(ref _selectedImageSource, value);
    }

    public bool HasSelectedImage
    {
        get => _hasSelectedImage;
        set => SetProperty(ref _hasSelectedImage, value);
    }

    public bool IsResultVisible
    {
        get => _isResultVisible;
        set => SetProperty(ref _isResultVisible, value);
    }

    public async Task LoadAsync()
    {
        try
        {
            ErrorMessage = null;

            // MVP: jedan korisnik, ID = 1
            var pots = await _apiService.GetPlantPotsByUserAsync(userId: 1);
            var pot = pots.FirstOrDefault();
            var plant = pot?.Plants.FirstOrDefault();

            if (plant is null)
            {
                _activePlantId = null;
                PlantDisplayName = "Nema aktivne biljke";
                StatusMessage = "Dodaj biljku u saksiju prije korištenja skenera.";
                return;
            }

            _activePlantId = plant.Id;

            PlantDisplayName = !string.IsNullOrWhiteSpace(plant.Nickname)
                ? plant.Nickname
                : plant.PlantTypeName ?? "Paradajz";

            StatusMessage = "Odaberi ili uslikaj list paradajza za analizu.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri učitavanju biljke: {ex.Message}";
        }
    }

    private async Task PickImageAsync()
    {
        try
        {
            ErrorMessage = null;
            IsResultVisible = false;

            var image = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Odaberi sliku lista paradajza"
            });

            if (image is null)
                return;

            await SetSelectedImageAsync(image);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri odabiru slike: {ex.Message}";
        }
    }

    private async Task TakePhotoAsync()
    {
        try
        {
            ErrorMessage = null;
            IsResultVisible = false;

            var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

            if (cameraStatus != PermissionStatus.Granted)
            {
                ErrorMessage = "Dozvola za kameru nije odobrena.";
                return;
            }

            if (!MediaPicker.Default.IsCaptureSupported)
            {
                ErrorMessage = "Kamera nije podržana na ovom uređaju/emulatoru.";
                return;
            }

            var image = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Uslikaj list paradajza"
            });

            if (image is null)
                return;

            await SetSelectedImageAsync(image);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Greška pri fotografisanju: {ex.Message}";
        }
    }

    private async Task SetSelectedImageAsync(FileResult image)
    {
        _selectedImageFile = image;

        await using var stream = await image.OpenReadAsync();
        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        var imageBytes = memoryStream.ToArray();

        SelectedImageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        HasSelectedImage = true;
        StatusMessage = "Slika je spremna za analizu.";
    }

    private async Task AnalyzeAsync()
    {
        if (IsBusy)
            return;

        if (_activePlantId is null)
        {
            ErrorMessage = "Nije pronađena aktivna biljka za analizu.";
            return;
        }

        if (_selectedImageFile is null)
        {
            ErrorMessage = "Prvo odaberi ili uslikaj sliku lista.";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = null;
            IsResultVisible = false;

            StatusMessage = "Analiza slike je u toku...";

            var result = await _apiService.AnalyzeDiseaseAsync(
                _activePlantId.Value,
                _selectedImageFile
            );

            if (result is null)
            {
                ErrorMessage = "Backend nije vratio rezultat analize.";
                StatusMessage = "Analiza nije završena.";
                return;
            }

            ResultTitle = BuildResultTitle(result);
            ConfidenceText = BuildConfidenceText(result.Confidence);
            Recommendation = result.TreatmentRecommendation
                ?? "Nema dostupne preporuke za ovu detekciju.";

            IsResultVisible = true;
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

    private static string BuildResultTitle(DiseaseDetectionResponse result)
    {
        if (result.IsHealthy)
            return "Biljka izgleda zdravo";

        var diseaseName = result.DiseaseNameLocal ?? result.DiseaseName ?? "Nepoznata bolest";
        var confidence = result.Confidence ?? 0;

        return confidence switch
        {
            >= 0.75m => $"Detektovana bolest: {diseaseName}",
            >= 0.50m => $"Moguća bolest: {diseaseName}",
            _ => $"Niska pouzdanost: {diseaseName}"
        };
    }

    private static string BuildConfidenceText(decimal? confidence)
    {
        if (confidence is null)
            return "Pouzdanost modela: nije dostupna";

        return $"Pouzdanost modela: {confidence * 100:0.#}%";
    }
}