using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class MyPotsPage : ContentPage
{
    private readonly MyPotsViewModel _viewModel;

    public MyPotsPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<MyPotsViewModel>();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }
}