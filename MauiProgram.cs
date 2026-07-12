using bashta_mobile.Constants;
using bashta_mobile.Infrastructure;
using bashta_mobile.Services;
using bashta_mobile.ViewModels;
using Microsoft.Extensions.Logging;

namespace bashta_mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiConstants.BaseUrl)
        });

        builder.Services.AddSingleton<IApiService, ApiService>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<PlantScannerViewModel>();
        builder.Services.AddTransient<MyPotsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AddPotViewModel>();
        builder.Services.AddTransient<EditPotViewModel>();
        builder.Services.AddTransient<WateringViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        AppServiceProvider.Initialize(app.Services);

        return app;
    }
}