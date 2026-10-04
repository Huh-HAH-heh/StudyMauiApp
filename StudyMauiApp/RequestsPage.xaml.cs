using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class RequestsPage : ContentPage
{
    private readonly ObservableCollection<RequestItem> _visibleRequests = [];
    private RequestType _requestType = RequestType.Adoption;
    private AnimalItem? _selectedAnimal;

    public RequestsPage()
    {
        InitializeComponent();

        RequestDatePicker.Date = DateTime.Today;

        LoadClientData();
        LoadAvailableAnimals();
        ApplyRoleUi();
        UpdateRequestTypeUi();
        LoadRequests();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadClientData();
        LoadAvailableAnimals();
        ApplyRoleUi();
        UpdateRequestTypeUi();
        LoadRequests();
    }

    private void ApplyRoleUi()
    {
        bool isClient = SessionObject.IsClient;
        bool isEmployee = SessionObject.IsAdmin || SessionObject.IsVolonteer;

        CreateRequestCard.IsVisible = isClient;

        if (isClient)
            PageSubtitle.Text = "Оформление заявки на прием или взятие питомца в семью.";
        else if (isEmployee)
            PageSubtitle.Text = "Просмотр заявок и оформление выдачи питомца в семью.";
        else
            PageSubtitle.Text = "Раздел заявок доступен клиентам и сотрудникам приюта.";
    }

    private void LoadClientData()
    {
        var user = SessionObject.CurrentUser;

        ClientNameEntry.Text = user?.FullName ?? "";
        PhoneEntry.Text = user?.Phone ?? "";
    }

    private void LoadAvailableAnimals()
    {
        AnimalOptions.Children.Clear();
        _selectedAnimal = null;
        SelectedAnimalLabel.Text = "Питомец не выбран";

        var animals = AnimalStore.Animals
            .Where(a => a.Status != "На лечении")
            .ToList();

        foreach (var animal in animals)
        {
            var button = new Button
            {
                Text = animal.DisplayName,
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                TextColor = Color.FromArgb("#20252B"),
                BorderColor = Color.FromArgb("#E0E5EA"),
                BorderWidth = 1,
                CornerRadius = 8,
                HeightRequest = 42,
                Padding = new Thickness(12, 6)
            };

            button.Clicked += (_, _) => SelectAnimal(animal, button);
            AnimalOptions.Children.Add(button);
        }

        if (animals.Count == 0)
        {
            AnimalOptions.Children.Add(new Label
            {
                Text = "Нет доступных питомцев.",
                TextColor = Color.FromArgb("#69747F"),
                FontSize = 12,
                Padding = new Thickness(4)
            });
        }
    }

    private void SelectAnimal(AnimalItem animal, Button selectedButton)
    {
        foreach (var child in AnimalOptions.Children.OfType<Button>())
        {
            child.BackgroundColor = Color.FromArgb("#F8FAFC");
            child.TextColor = Color.FromArgb("#20252B");
        }

        selectedButton.BackgroundColor = Color.FromArgb("#2F80ED");
        selectedButton.TextColor = Colors.White;

        _selectedAnimal = animal;
        SelectedAnimalLabel.Text = $"Выбран: {animal.Name} ({animal.Number})";
    }

    private void OnAdoptionTypeClicked(object? sender, EventArgs e)
    {
        _requestType = RequestType.Adoption;
        UpdateRequestTypeUi();
    }

    private void OnSurrenderTypeClicked(object? sender, EventArgs e)
    {
        _requestType = RequestType.Surrender;
        UpdateRequestTypeUi();
    }

    private void UpdateRequestTypeUi()
    {
        bool adoption = _requestType == RequestType.Adoption;

        AdoptionFields.IsVisible = adoption;
        SurrenderFields.IsVisible = !adoption;

        if (adoption)
        {
            AdoptionTypeButton.BackgroundColor = Color.FromArgb("#2F80ED");
            AdoptionTypeButton.TextColor = Colors.White;

            SurrenderTypeButton.BackgroundColor = Color.FromArgb("#E9EEF3");
            SurrenderTypeButton.TextColor = Color.FromArgb("#20252B");
        }
        else
        {
            AdoptionTypeButton.BackgroundColor = Color.FromArgb("#E9EEF3");
            AdoptionTypeButton.TextColor = Color.FromArgb("#20252B");

            SurrenderTypeButton.BackgroundColor = Color.FromArgb("#2F80ED");
            SurrenderTypeButton.TextColor = Colors.White;
        }
    }

    private void LoadRequests()
    {
        _visibleRequests.Clear();

        var user = SessionObject.CurrentUser;
        IEnumerable<RequestItem> requests = RequestStore.Requests;

        if (SessionObject.IsClient && user is not null)
        {
            requests = requests.Where(r =>
                r.ClientName.Equals(user.FullName, StringComparison.OrdinalIgnoreCase) ||
                r.Phone.Equals(user.Phone, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var request in requests
                     .OrderByDescending(r => r.Date)
                     .ThenByDescending(r => r.Id))
        {
            _visibleRequests.Add(request);
        }

        RequestsView.ItemsSource = _visibleRequests;
        RequestsCountLabel.Text = $"Всего заявок: {_visibleRequests.Count}";
    }

    private async void OnCreateRequestClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (!SessionObject.IsClient)
        {
            await DisplayAlert(
                "Нет доступа",
                "Заявку на прием или взятие животного оформляет клиент.",
                "ОК");
            return;
        }

        var user = SessionObject.CurrentUser;

        if (user is null)
        {
            await DisplayAlert("Ошибка", "Сначала войдите в систему.", "ОК");
            return;
        }

        string animalNumber = "";
        string animalName = "";
        string animalDescription = "";

        if (_requestType == RequestType.Adoption)
        {
            if (_selectedAnimal is null)
            {
                ValidationLabel.Text = "Выберите питомца, которого хотите взять в семью.";
                ValidationLabel.IsVisible = true;
                return;
            }

            animalNumber = _selectedAnimal.Number;
            animalName = _selectedAnimal.Name;
        }
        else
        {
            animalDescription = SurrenderAnimalEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(animalDescription))
            {
                ValidationLabel.Text = "Опишите животное, которое хотите передать в приют.";
                ValidationLabel.IsVisible = true;
                return;
            }
        }

        int id = RequestStore.Requests.Count == 0
            ? 1
            : RequestStore.Requests.Max(r => r.Id) + 1;

        RequestStore.Requests.Add(
            new RequestItem
            {
                Id = id,
                ClientName = user.FullName,
                Phone = user.Phone,
                TypeValue = _requestType,
                AnimalNumber = animalNumber,
                AnimalName = animalName,
                AnimalDescription = animalDescription,
                Date = RequestDatePicker.Date
            });

        await DisplayAlert(
            "Заявка оформлена",
            $"Заявка №{id} зарегистрирована.",
            "ОК");

        SurrenderAnimalEntry.Text = "";
        LoadAvailableAnimals();
        LoadRequests();
    }

    private async void OnProcessClicked(object? sender, EventArgs e)
    {
        if (!SessionObject.IsAdmin && !SessionObject.IsVolonteer)
        {
            await DisplayAlert(
                "Нет доступа",
                "Оформить выдачу могут администратор и волонтер.",
                "ОК");
            return;
        }

        if (sender is not Button button ||
            button.CommandParameter is not RequestItem request)
            return;

        var animal = AnimalStore.Animals
            .FirstOrDefault(a =>
                a.Number.Equals(request.AnimalNumber, StringComparison.OrdinalIgnoreCase));

        if (animal is null)
        {
            await DisplayAlert(
                "Ошибка",
                "Питомец из заявки не найден в журнале.",
                "ОК");
            return;
        }

        if (animal.Status == "На лечении")
        {
            await DisplayAlert(
                "Нельзя оформить выдачу",
                "Животное находится на лечении.",
                "ОК");
            return;
        }

        bool confirm = await DisplayAlert(
            "Оформление выдачи",
            $"Передать «{animal.Name}» клиенту {request.ClientName}?",
            "Оформить",
            "Отмена");

        if (!confirm)
            return;

        AnimalStore.Animals.Remove(animal);
        AnimalStore.AdoptedCount++;
        request.Status = "Выдано в семью";

        await DisplayAlert(
            "Готово",
            $"Животное «{animal.Name}» выдано в семью.",
            "ОК");

        LoadRequests();
    }

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");
}