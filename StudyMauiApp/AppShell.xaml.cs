namespace StudyMauiApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("AddAnimalPage", typeof(AddAnimalPage));
        Routing.RegisterRoute("RequestsPage", typeof(RequestsPage));
        Routing.RegisterRoute("VeterinaryPage", typeof(VeterinaryPage));
    }
}