using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<DashboardViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
}