using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class WateringPage : ContentPage
{
    private readonly WateringViewModel _viewModel;

    public WateringPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<WateringViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
}