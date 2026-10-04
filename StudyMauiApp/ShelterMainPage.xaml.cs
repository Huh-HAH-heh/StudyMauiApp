using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class ShelterMainPage : ContentPage
{
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
        LoadRecentActivity();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ApplyRoleUi();
        ApplyFilters();
        UpdateStats();
        LoadRecentActivity();
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

    private void LoadRecentActivity()
    {
        RecentRequestsView.ItemsSource = RequestStore.Requests
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Id)
            .Take(3)
            .ToList();

        RecentVetView.ItemsSource = VeterinaryStore.Records
            .OrderByDescending(v => v.Date)
            .Take(3)
            .ToList();

        RecentRequestsCountLabel.Text =
            RequestStore.Requests.Count == 0
                ? "Заявок пока нет"
                : $"Всего заявок: {RequestStore.Requests.Count}";
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
            await DisplayAlert("Нет доступа", "Раздел заявок доступен клиенту и сотрудникам приюта.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("RequestsPage");
    }

    private async void OnVetClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsVeterenar)
        {
            await DisplayAlert("Нет доступа", "Медицинские манипуляции доступны ветеринарному врачу.", "ОК");
            return;
        }

        await Shell.Current.GoToAsync("VeterinaryPage");
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

        await Shell.Current.GoToAsync("RequestsPage");
    }
}