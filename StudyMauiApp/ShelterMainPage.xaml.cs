using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class ShelterMainPage : ContentPage
{
    bool isAdmin;
    public class User
    {
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
        public UserRole Role { get; set; }
    }
public enum UserRole
    {
        Anonim ,
        Client,
        Admin,
        Volonteer,
        Veterenar
    }
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    public static class UserStore
    {
        public static List<User> Users { get; } = [
            new User{
                   FullName = "Nerd",
            Phone = "+7 666 666-66-66",
            Login = "Nerd",
            Password = "Nerd",
            Role = UserRole.Admin
            },
                new User{
                   FullName = "GOON",
            Phone = "+7 900 000-00-00",
            Login = "GOON",
            Password = "GOON",
            Role = UserRole.Client
            },         new User{
                   FullName = "WERDO",
            Phone = "+7 900 000-00-00",
            Login = "WERDO",
            Password = "WERDO",
            Role = UserRole.Volonteer
            }
            ];
    }
    public static class SessionObject
    {
        public static User? CurrentUser { get; set; }

        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsVolonteer => CurrentUser?.Role == UserRole.Volonteer;
        public static bool IsClient => CurrentUser?.Role == UserRole.Client;
        //public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        
    }
    private readonly List<AnimalItem> _animals =
    [
        new("Белка", "A-104", "Собака • Лабрадор • Самка • 3 года", "В приюте", "24 кг"),
        new("Рекс", "A-107", "Собака • Овчарка • Самец • 5 лет", "В приюте", "31 кг"),
        new("Барсик", "A-112", "Кошка • Британская • Самец • 2 года", "На лечении", "5 кг"),
        new("Мурка", "A-118", "Кошка • Дворовая • Самка • 1 год", "Ожидает семью", "4 кг"),
        new("Лада", "A-121", "Собака • Метис • Самка • 4 года", "В приюте", "18 кг"),
        new("Тузик", "A-125", "Собака • Метис • Самец • 7 лет", "Ожидает семью", "22 кг")
    ];

    private readonly ObservableCollection<AnimalItem> _filteredAnimals = [];

    public ShelterMainPage()
    {
        InitializeComponent();
        LoginButton.Clicked += OnLoginClicked;

        SpeciesPicker.SelectedIndex = 0;
        GenderPicker.SelectedIndex = 0;

        ApplyFilters();
    }

    private void OnFilterChanged(object? sender, EventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        _filteredAnimals.Clear();

        string search = SearchBox?.Text?.Trim() ?? string.Empty;
        string species = SpeciesPicker?.SelectedItem?.ToString() ?? "Все виды";
        string gender = GenderPicker?.SelectedItem?.ToString() ?? "Любой пол";
        string breed = BreedBox?.Text?.Trim() ?? string.Empty;

        foreach (var animal in _animals)
        {
            bool matchesSearch =
                string.IsNullOrWhiteSpace(search) ||
                animal.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                animal.Number.Contains(search, StringComparison.OrdinalIgnoreCase);

            bool matchesSpecies =
                species == "Все виды" ||
                animal.Details.Contains(species, StringComparison.OrdinalIgnoreCase);

            bool matchesGender =
                gender == "Любой пол" ||
                animal.Details.Contains(gender, StringComparison.OrdinalIgnoreCase);

            bool matchesBreed =
                string.IsNullOrWhiteSpace(breed) ||
                animal.Details.Contains(breed, StringComparison.OrdinalIgnoreCase);

            if (matchesSearch && matchesSpecies && matchesGender && matchesBreed)
                _filteredAnimals.Add(animal);
        }

        AnimalsView.ItemsSource = _filteredAnimals;
        ResultsLabel.Text = $"Найдено: {_filteredAnimals.Count}";
    }

    private async void OnAddAnimalClicked(object? sender, EventArgs e) =>
        await DisplayAlert("Регистрация животного",
            "Здесь будет форма регистрации нового питомца: вид, пол, порода, окрас, возраст, вес, фото и прививки.",
            "ОК");

    private async void OnAddRequestClicked(object? sender, EventArgs e) =>
        await DisplayAlert("Заявка",
            "Здесь будет оформление заявки на сдачу животного или его передачу в семью.",
            "ОК");

    private async void OnVetClicked(object? sender, EventArgs e) =>
        await DisplayAlert("Ветеринария",
            "Здесь будет журнал медицинских манипуляций с датой, видом процедуры и животным.",
            "ОК");

    private async void OnReportsClicked(object? sender, EventArgs e) =>
        await DisplayAlert("Отчет",
            "Количество содержащихся животных: 42\nКоличество отданных в семьи: 17",
            "ОК");

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (AnimalsView.SelectedItem is AnimalItem animal)
            await DisplayAlert("Карточка питомца",
                $"{animal.Name} ({animal.Number})\n{animal.Details}\nВес: {animal.Weight}",
                "ОК");
        else
            await DisplayAlert("Карточка питомца", "Выберите животное из списка.", "ОК");
    }

    private async void OnAnimalSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AnimalItem animal)
        {
            await DisplayAlert(animal.Name,
                $"Номер: {animal.Number}\n{animal.Details}\nСтатус: {animal.Status}\nВес: {animal.Weight}",
                "ОК");

            AnimalsView.SelectedItem = null;
        }
    }

    private void OnResetFiltersClicked(object? sender, EventArgs e)
    {
        SearchBox.Text = string.Empty;
        BreedBox.Text = string.Empty;
        SpeciesPicker.SelectedIndex = 0;
        GenderPicker.SelectedIndex = 0;
        ApplyFilters();
    }

    private async void OnHomeClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");

    private async void OnRequestsClicked(object? sender, EventArgs e) =>
        await DisplayAlert("Заявки", "Раздел заявок подготовлен как часть интерфейса ТЗ.", "ОК");

    private sealed record AnimalItem(
        string Name,
        string Number,
        string Details,
        string Status,
        string Weight);
}
