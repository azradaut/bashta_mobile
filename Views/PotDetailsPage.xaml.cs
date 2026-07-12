using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class PotDetailsPage : ContentPage, IQueryAttributable
{
    private readonly PotDetailsViewModel _viewModel;

    public PotDetailsPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<PotDetailsViewModel>();
        BindingContext = _viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Pot", out var potObject) &&
            potObject is PlantPotCardViewModel pot)
        {
            await _viewModel.LoadFromPotAsync(pot);
        }
    }
}