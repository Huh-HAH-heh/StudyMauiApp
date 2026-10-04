using System.Collections.ObjectModel;
using System.Globalization;

namespace StudyMauiApp;

public partial class VeterinaryPage : ContentPage
{
    private readonly ObservableCollection<VeterinaryItem> _visibleRecords = [];
    private AnimalItem? _selectedAnimal;

    public VeterinaryPage()
    {
        InitializeComponent();

        DateEntry.Text = DateTime.Today.ToString("dd.MM.yyyy");

        LoadAnimals();
        ApplyRoleUi();
        LoadRecords();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadAnimals();
        ApplyRoleUi();
        LoadRecords();
    }

    private void ApplyRoleUi()
    {
        bool isVet = SessionObject.IsVeterenar;

        CreateRecordCard.IsVisible = isVet;

        PageSubtitle.Text = isVet
            ? "Фиксация даты, вида медицинской манипуляции и данных о животном."
            : "Журнал медицинских манипуляций.";
    }

    private void LoadAnimals()
    {
        AnimalOptions.Children.Clear();
        _selectedAnimal = null;
        SelectedAnimalLabel.Text = "Животное не выбрано";

        foreach (var animal in AnimalStore.Animals)
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

        if (AnimalStore.Animals.Count == 0)
        {
            AnimalOptions.Children.Add(new Label
            {
                Text = "В приюте нет зарегистрированных животных.",
                TextColor = Color.FromArgb("#69747F"),
                FontSize = 12
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
        SelectedAnimalLabel.Text = $"Выбрано: {animal.Name} ({animal.Number})";
    }

    private void LoadRecords()
    {
        _visibleRecords.Clear();

        foreach (var record in VeterinaryStore.Records
                     .OrderByDescending(r => r.Date))
        {
            _visibleRecords.Add(record);
        }

        RecordsView.ItemsSource = _visibleRecords;
        RecordsCountLabel.Text = $"Всего записей: {_visibleRecords.Count}";
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (!SessionObject.IsVeterenar)
        {
            await DisplayAlert(
                "Нет доступа",
                "Медицинские манипуляции может фиксировать ветеринарный врач.",
                "ОК");
            return;
        }

        if (_selectedAnimal is null)
        {
            ValidationLabel.Text = "Выберите животное.";
            ValidationLabel.IsVisible = true;
            return;
        }

        string procedure = ProcedureEntry.Text?.Trim() ?? "";
        string notes = NotesEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(procedure))
        {
            ValidationLabel.Text = "Укажите вид медицинской манипуляции.";
            ValidationLabel.IsVisible = true;
            return;
        }

        if (!DateTime.TryParseExact(
                DateEntry.Text?.Trim(),
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            ValidationLabel.Text = "Введите дату в формате ДД.ММ.ГГГГ.";
            ValidationLabel.IsVisible = true;
            return;
        }

        VeterinaryStore.Records.Add(
            new VeterinaryItem(
                _selectedAnimal.Name,
                _selectedAnimal.Number,
                procedure,
                date,
                string.IsNullOrWhiteSpace(notes) ? "Без дополнительных сведений" : notes));

        await DisplayAlert(
            "Готово",
            $"Запись о манипуляции «{procedure}» для «{_selectedAnimal.Name}» сохранена.",
            "ОК");

        ProcedureEntry.Text = "";
        NotesEditor.Text = "";
        DateEntry.Text = DateTime.Today.ToString("dd.MM.yyyy");

        LoadRecords();
    }

    private async void OnBackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");
}