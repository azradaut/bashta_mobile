using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class AddPotPage : ContentPage
{
    private readonly AddPotViewModel _viewModel;

    public AddPotPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<AddPotViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
}