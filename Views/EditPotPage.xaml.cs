using bashta_mobile.Infrastructure;
using bashta_mobile.ViewModels;

namespace bashta_mobile.Views;

public partial class EditPotPage : ContentPage, IQueryAttributable
{
    private readonly EditPotViewModel _viewModel;

    public EditPotPage()
    {
        InitializeComponent();

        _viewModel = AppServiceProvider.GetRequiredService<EditPotViewModel>();
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