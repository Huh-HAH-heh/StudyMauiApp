namespace StudyMauiApp;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void onLoginClicked(object? sender, EventArgs e)
    {
        string login = LoginEntry.Text?.Trim() ?? "";
        string pass = Pass.Text ?? "";

        var user = UserStore.Users.FirstOrDefault(item =>
            item.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
            item.Password == pass);

        if (user is null)
        {
            await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
            return;
        }

        SessionObject.CurrentUser = user;
        await Shell.Current.GoToAsync("..");
    }
}