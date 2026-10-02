


namespace StudyMauiApp;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }
    private async void onLoginClicked(object? sender, EventArgs e)
    {
        String login = LoginEntry.Text;
        string pass = Pass.Text;
        ShelterMainPage.User? user = null;
        foreach (var item in ShelterMainPage.UserStore.Users)
        {
          
           
                if (item.Login== login && item.Password== pass)
                {
                    user = item;
                break;
            }


        }
        if (user == null)
        {
            await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
            return;
        }
    

            ShelterMainPage.SessionObject.CurrentUser = user;
            await Shell.Current.GoToAsync("..");
        

    } 
}
