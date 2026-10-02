


namespace StudyMauiApp;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void onLoginClicked(object? sender , EventArgs e)
    {
        String login = LoginEntry.Text;
            string pass = Pass.Text;
        await DisplayAlert(
            "Вход", $"Логин {login}\nПароль:{pass}",
            "OK");

    }
}
