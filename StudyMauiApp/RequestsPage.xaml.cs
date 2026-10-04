using System.Collections.ObjectModel;

namespace StudyMauiApp;

public partial class RequestsPage : ContentPage
{
    public enum RequestType
    {
        Adoption,
        Surrender
    }

    public sealed class RequestItem
    {
        public int Id { get; init; }
        public string ClientName { get; init; } = "";
        public string Phone { get; init; } = "";
        public RequestType TypeValue { get; init; }
        public string AnimalNumber { get; init; } = "";
        public string AnimalName { get; init; } = "";
        public string AnimalDescription { get; init; } = "";
        public DateTime Date { get; init; }
        public string Status { get; set; } = "Новая";

        public string Type =>
            TypeValue == RequestType.Adoption
                ? "Взятие животного в семью"
                : "Сдача животного в приют";

        public string DateText => Date.ToString("dd.MM.yyyy");

        public string AnimalText =>
            TypeValue == RequestType.Adoption
                ? $"Питомец: {AnimalName} ({AnimalNumber})"
                : $"Животное: {AnimalDescription}";

        public bool CanProcess =>
            TypeValue == RequestType.Adoption &&
            Status == "Новая" &&
            (ShelterMainPage.SessionObject.IsAdmin ||
             ShelterMainPage.SessionObject.IsVolonteer);
    }

    public static class RequestStore
    {
        public static List<RequestItem> Requests { get; } =
        [
            new RequestItem
            {
                Id = 1,
                ClientName = "Иванова Анна",
                Phone = "+7 900 111-11-11",
                TypeValue = RequestType.Adoption,
                AnimalNumber = "A-118",
                AnimalName = "Мурка",
                Date = new DateTime(2026, 10, 2)
            },
            new RequestItem
            {
                Id = 2,
                ClientName = "Петров Иван",
                Phone = "+7 900 222-22-22",
                TypeValue = RequestType.Surrender,
                AnimalDescription = "Собака, метис, самец, около 5 лет",
                Date = new DateTime(2026, 10, 1)
            }
        ];
    }

    private readonly ObservableCollection<RequestItem> _visibleRequests = [];

    public RequestsPage()
    {
        InitializeComponent();

        RequestDatePicker.Date = DateTime.Today;
        RequestTypePicker.SelectedIndex = 0;

        LoadClientData();
        LoadAvailableAnimals();
        ApplyRoleUi();
        LoadRequests();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadClientData();
        LoadAvailableAnimals();
        ApplyRoleUi();
        LoadRequests();
    }

    private void ApplyRoleUi()
    {
        bool isClient = ShelterMainPage.SessionObject.IsClient;
        bool isEmployee =
            ShelterMainPage.SessionObject.IsAdmin ||
            ShelterMainPage.SessionObject.IsVolonteer;

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
        var user = ShelterMainPage.SessionObject.CurrentUser;

        ClientNameEntry.Text = user?.FullName ?? "";
        PhoneEntry.Text = user?.Phone ?? "";
    }

    private void LoadAvailableAnimals()
    {
        AnimalPicker.ItemsSource = ShelterMainPage.AnimalStore.Animals
            .Where(a => a.Status != "На лечении")
            .Select(a => $"{a.Number} — {a.Name}")
            .ToList();

        AnimalPicker.SelectedIndex =
            AnimalPicker.Items.Count > 0 ? 0 : -1;
    }

    private void OnRequestTypeChanged(object? sender, EventArgs e)
    {
        bool adoption = RequestTypePicker.SelectedIndex == 0;

        AdoptionFields.IsVisible = adoption;
        SurrenderFields.IsVisible = !adoption;
    }

    private void LoadRequests()
    {
        _visibleRequests.Clear();

        var user = ShelterMainPage.SessionObject.CurrentUser;
        IEnumerable<RequestItem> requests = RequestStore.Requests;

        if (ShelterMainPage.SessionObject.IsClient && user is not null)
        {
            requests = requests.Where(r =>
                r.ClientName.Equals(user.FullName, StringComparison.OrdinalIgnoreCase) ||
                r.Phone.Equals(user.Phone, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var request in requests.OrderByDescending(r => r.Date).ThenByDescending(r => r.Id))
            _visibleRequests.Add(request);

        RequestsView.ItemsSource = _visibleRequests;
        RequestsCountLabel.Text = $"Всего заявок: {_visibleRequests.Count}";
    }

    private async void OnCreateRequestClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (!ShelterMainPage.SessionObject.IsClient)
        {
            await DisplayAlert(
                "Нет доступа",
                "Заявку на прием или взятие животного оформляет клиент.",
                "ОК");
            return;
        }

        var user = ShelterMainPage.SessionObject.CurrentUser;

        if (user is null)
        {
            await DisplayAlert("Ошибка", "Сначала войдите в систему.", "ОК");
            return;
        }

        var type = RequestTypePicker.SelectedIndex == 0
            ? RequestType.Adoption
            : RequestType.Surrender;

        string animalNumber = "";
        string animalName = "";
        string animalDescription = "";

        if (type == RequestType.Adoption)
        {
            if (AnimalPicker.SelectedItem is not string selected ||
                string.IsNullOrWhiteSpace(selected))
            {
                ValidationLabel.Text = "Выберите питомца, которого хотите взять в семью.";
                ValidationLabel.IsVisible = true;
                return;
            }

            int separator = selected.IndexOf(" — ", StringComparison.Ordinal);

            if (separator < 0)
            {
                ValidationLabel.Text = "Не удалось определить выбранного питомца.";
                ValidationLabel.IsVisible = true;
                return;
            }

            animalNumber = selected[..separator];
            animalName = selected[(separator + 3)..];
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
                TypeValue = type,
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
        LoadRequests();
    }

    private async void OnProcessClicked(object? sender, EventArgs e)
    {
        if (!ShelterMainPage.SessionObject.IsAdmin &&
            !ShelterMainPage.SessionObject.IsVolonteer)
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

        var animal = ShelterMainPage.AnimalStore.Animals
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

        ShelterMainPage.AnimalStore.Animals.Remove(animal);
        ShelterMainPage.AnimalStore.AdoptedCount++;
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