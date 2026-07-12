using bashta_mobile.Views;

namespace bashta_mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("add-pot", typeof(AddPotPage));
        Routing.RegisterRoute("edit-pot", typeof(EditPotPage));
        Routing.RegisterRoute("pot-details", typeof(PotDetailsPage));
    }
}