using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class ShelterMainPage : ContentPage
{
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
        Anonim,
        Client,
        Admin,
        Volonteer,
        Veterenar
    }

    public static class UserStore
    {
        public static List<User> Users { get; } =
        [
            new User
            {
                FullName = "Nerd",
                Phone = "+7 666 666-66-66",
                Login = "Nerd",
                Password = "Nerd",
                Role = UserRole.Admin
            },
            new User
            {
                FullName = "GOON",
                Phone = "+7 900 000-00-00",
                Login = "GOON",
                Password = "GOON",
                Role = UserRole.Client
            },
            new User
            {
                FullName = "WERDO",
                Phone = "+7 900 000-00-00",
                Login = "WERDO",
                Password = "WERDO",
                Role = UserRole.Volonteer
            },
            new User
            {
                FullName = "Доктор Иванов",
                Phone = "+7 901 111-11-11",
                Login = "vet",
                Password = "vet",
                Role = UserRole.Veterenar
            }
        ];
    }

    public static class SessionObject
    {
        private static User? _currentUser;

        public static User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                Console.WriteLine($"CurrentUser изменён: {value?.Login ?? "Anonim"}");
            }
        }

        public static bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
        public static bool IsVolonteer => CurrentUser?.Role == UserRole.Volonteer;
        public static bool IsClient => CurrentUser?.Role == UserRole.Client;
        public static bool IsVeterenar => CurrentUser?.Role == UserRole.Veterenar;
        public static bool IsAnonymous => CurrentUser is null;
    }

    public static class AnimalStore
    {
        public static List<AnimalItem> Animals { get; } =
        [
            new("Белка", "A-104", "Собака", "Лабрадор", "Самка", "Серый", "Средний", "3 года", "24 кг", "Вакцинация от бешенства", "В приюте", "Не прикреплено"),
            new("Рекс", "A-107", "Собака", "Овчарка", "Самец", "Черно-рыжий", "Крупный", "5 лет", "31 кг", "Вакцинация выполнена", "В приюте", "Не прикреплено"),
            new("Барсик", "A-112", "Кошка", "Британская", "Самец", "Серый", "Средний", "2 года", "5 кг", "Наблюдение после лечения", "На лечении", "Не прикреплено"),
            new("Мурка", "A-118", "Кошка", "Дворовая", "Самка", "Черно-белый", "Мелкий", "1 год", "4 кг", "Прививки отсутствуют", "Ожидает семью", "Не прикреплено"),
            new("Лада", "A-121", "Собака", "Метис", "Самка", "Белый", "Средний", "4 года", "18 кг", "Вакцинация выполнена", "В приюте", "Не прикреплено"),
            new("Тузик", "A-125", "Собака", "Метис", "Самец", "Коричневый", "Крупный", "7 лет", "22 кг", "Вакцинация выполнена", "Ожидает семью", "Не прикреплено")
        ];

        public static int AdoptedCount { get; set; } = 17;
    }

    private readonly ObservableCollection<AnimalItem> _filteredAnimals = [];

    public ShelterMainPage()
    {
        InitializeComponent();

        LoginButton.Clicked += OnLoginClicked;

        SpeciesPicker.SelectedIndex = 0;
        GenderPicker.SelectedIndex = 0;

        ApplyRoleUi();
        ApplyFilters();
        UpdateStats();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyRoleUi();
        ApplyFilters();
        UpdateStats();
    }

    private void ApplyRoleUi()
    {
        var user = SessionObject.CurrentUser;

        CurrentUserLabel.Text = user?.FullName ?? "";
        CurrentRoleLabel.Text = user is null ? "" : GetRoleName(user.Role);

        CurrentUserLabel.IsVisible = user is not null;
        CurrentRoleLabel.IsVisible = user is not null;
        LoginButton.Text = user is null ? "Войти" : "Сменить пользователя";

        AddAnimalButton.IsVisible = SessionObject.IsAdmin || SessionObject.IsVolonteer;
        RequestButton.IsVisible = SessionObject.IsAdmin || SessionObject.IsVolonteer || SessionObject.IsClient;
        VetButton.IsVisible = SessionObject.IsVeterenar;
        ReportsButton.IsVisible = SessionObject.IsAdmin;

        ReportsNavButton.IsVisible = SessionObject.IsAdmin;
        RequestsNavButton.IsVisible = SessionObject.IsAdmin || SessionObject.IsVolonteer || SessionObject.IsClient;
    }

    private static string GetRoleName(UserRole role) => role switch
    {
        UserRole.Admin => "Администратор",
        UserRole.Client => "Клиент",
        UserRole.Volonteer => "Волонтер",
        UserRole.Veterenar => "Ветеринарный врач",
        _ => "Гость"
    };

    private void UpdateStats()
    {
        AnimalsCountLabel.Text = AnimalStore.Animals.Count.ToString();
        AdoptedCountLabel.Text = AnimalStore.AdoptedCount.ToString();
        TreatmentCountLabel.Text = AnimalStore.Animals.Count(a => a.Status == "На лечении").ToString();
        WaitingCountLabel.Text = AnimalStore.Animals.Count(a => a.Status == "Ожидает семью").ToString();
    }

    private bool CanManageAnimals() =>
        SessionObject.IsAdmin || SessionObject.IsVolonteer;

    private bool CanCreateRequest() =>
        SessionObject.IsAdmin || SessionObject.IsVolonteer || SessionObject.IsClient;

    private async void OnLoginClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("LoginPage");

    private void OnFilterChanged(object? sender, EventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        _filteredAnimals.Clear();

        string search = SearchBox?.Text?.Trim() ?? string.Empty;
        string species = SpeciesPicker?.SelectedItem?.ToString() ?? "Все виды";
        string gender = GenderPicker?.SelectedItem?.ToString() ?? "Любой пол";
        string breed = BreedBox?.Text?.Trim() ?? string.Empty;

        foreach (var animal in AnimalStore.Animals)
        {
            bool matchesSearch =
                string.IsNullOrWhiteSpace(search) ||
                animal.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                animal.Number.Contains(search, StringComparison.OrdinalIgnoreCase);

            bool matchesSpecies =
                species == "Все виды" ||
                animal.Species.Contains(species, StringComparison.OrdinalIgnoreCase);

            bool matchesGender =
                gender == "Любой пол" ||
                animal.Gender.Contains(gender, StringComparison.OrdinalIgnoreCase);

            bool matchesBreed =
                string.IsNullOrWhiteSpace(breed) ||
                animal.Breed.Contains(breed, StringComparison.OrdinalIgnoreCase);

            if (matchesSearch && matchesSpecies && matchesGender && matchesBreed)
                _filteredAnimals.Add(animal);
        }

        AnimalsView.ItemsSource = _filteredAnimals;
        ResultsLabel.Text = $"Найдено: {_filteredAnimals.Count}";
    }

    private async void OnAddAnimalClicked(object? sender, EventArgs e)
    {
        if (!CanManageAnimals())
        {
            await DisplayAlert("Нет доступа", "Регистрировать новых животных могут администратор и волонтер.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("AddAnimalPage");
    }

    private async void OnAddRequestClicked(object? sender, EventArgs e)
    {
        if (!CanCreateRequest())
        {
            await DisplayAlert("Нет доступа", "Создавать заявки могут клиент, администратор и волонтер.", "ОК");
            return;
        }

        await DisplayAlert("Заявка", "Форма заявки будет следующим отдельным разделом системы.", "ОК");
    }

    private async void OnVetClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsVeterenar)
        {
            await DisplayAlert("Нет доступа", "Медицинские манипуляции доступны ветеринарному врачу.", "ОК");
            return;
        }

        await DisplayAlert("Ветеринария", "Журнал медицинских манипуляций будет следующим отдельным разделом системы.", "ОК");
    }

    private async void OnReportsClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin)
        {
            await DisplayAlert("Нет доступа", "Отчеты доступны только администратору.", "ОК");
            return;
        }

        await DisplayAlert(
            "Отчет",
            $"Количество содержащихся животных: {AnimalStore.Animals.Count}\nКоличество отданных в семьи: {AnimalStore.AdoptedCount}",
            "ОК");
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (AnimalsView.SelectedItem is AnimalItem animal)
        {
            await DisplayAlert(
                "Карточка питомца",
                $"{animal.Name} ({animal.Number})\n" +
                $"Вид: {animal.Species}\n" +
                $"Порода: {animal.Breed}\n" +
                $"Пол: {animal.Gender}\n" +
                $"Окрас: {animal.Color}\n" +
                $"Размер: {animal.Size}\n" +
                $"Возраст: {animal.Age}\n" +
                $"Вес: {animal.Weight}\n" +
                $"Прививки: {animal.Vaccinations}\n" +
                $"Фото: {animal.Photo}\n" +
                $"Статус: {animal.Status}",
                "ОК");
        }
        else
        {
            await DisplayAlert("Карточка питомца", "Выберите животное из списка.", "ОК");
        }
    }

    private async void OnAnimalSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AnimalItem animal)
        {
            await DisplayAlert(
                animal.Name,
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

    private async void OnRequestsClicked(object? sender, EventArgs e)
    {
        if (!CanCreateRequest())
        {
            await DisplayAlert("Нет доступа", "Раздел заявок доступен клиенту и сотрудникам приюта.", "ОК");
            return;
        }

        await DisplayAlert("Заявки", "Раздел заявок подготовлен как следующий функциональный экран.", "ОК");
    }

    public sealed record AnimalItem(
        string Name,
        string Number,
        string Species,
        string Breed,
        string Gender,
        string Color,
        string Size,
        string Age,
        string Weight,
        string Vaccinations,
        string Status,
        string Photo)
    {
        public string Details => $"{Species} • {Breed} • {Gender} • {Age}";
    }
}