using System.Globalization;
using System.Windows.Input;
using bashta_mobile.Constants;

namespace bashta_mobile.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private const string LanguagePreferenceKey = "AppLanguage";

    private string _selectedLanguage = "Bosanski";
    private string _statusMessage = "Postavke aplikacije za MVP prototip.";

    public SettingsViewModel()
    {
        SetBosnianCommand = new Command(() => SetLanguage("bs", "Bosanski"));
        SetEnglishCommand = new Command(() => SetLanguage("en", "English"));

        LoadSettings();
    }

    public ICommand SetBosnianCommand { get; }
    public ICommand SetEnglishCommand { get; }

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set => SetProperty(ref _selectedLanguage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string BackendUrl => ApiConstants.BaseUrl;

    public string AppVersion => "1.0 MVP";

    private void LoadSettings()
    {
        var languageCode = Preferences.Default.Get(LanguagePreferenceKey, "bs");

        SelectedLanguage = languageCode == "en"
            ? "English"
            : "Bosanski";
    }

    private void SetLanguage(string languageCode, string languageName)
    {
        Preferences.Default.Set(LanguagePreferenceKey, languageCode);

        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        SelectedLanguage = languageName;

        StatusMessage = languageCode == "bs"
            ? "Jezik je postavljen na bosanski. Potpuna lokalizacija će se primijeniti kroz resursne fajlove."
            : "Language has been set to English. Full localization will be applied through resource files.";
    }
}