using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class PlantScannerPage : ContentPage
{
    private readonly PlantScannerViewModel _viewModel;

    public PlantScannerPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<PlantScannerViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
}