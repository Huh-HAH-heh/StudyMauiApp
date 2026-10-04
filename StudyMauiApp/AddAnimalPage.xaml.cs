namespace StudyMauiApp;

public partial class AddAnimalPage : ContentPage
{
    private FileResult? _photo;

    public AddAnimalPage()
    {
        InitializeComponent();
        AdmissionDatePicker.Date = DateTime.Today;
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            _photo = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите фотографию животного",
                FileTypes = FilePickerFileType.Images
            });

            PhotoLabel.Text = _photo?.FileName ?? "Фото не выбрано";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выбора фото: {ex.Message}");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ValidationLabel.IsVisible = false;

        if (SessionObject.CurrentUser is not
            { Role: UserRole.Admin or UserRole.Volonteer })
        {
            await DisplayAlert(
                "Нет доступа",
                "Регистрировать животных могут администратор и волонтер.",
                "ОК");
            return;
        }

        string name = NameEntry.Text?.Trim() ?? "";
        string number = NumberEntry.Text?.Trim() ?? "";
        string species = SpeciesEntry.Text?.Trim() ?? "";
        string gender = GenderEntry.Text?.Trim() ?? "";
        string breed = BreedEntry.Text?.Trim() ?? "";
        string color = ColorEntry.Text?.Trim() ?? "";
        string size = SizeEntry.Text?.Trim() ?? "";
        string age = AgeEntry.Text?.Trim() ?? "";
        string weight = WeightEntry.Text?.Trim() ?? "";
        string vaccinations = VaccinationsEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(number))
        {
            ValidationLabel.Text = "Заполните обязательные поля: кличка и идентификационный номер.";
            ValidationLabel.IsVisible = true;
            return;
        }

        if (string.IsNullOrWhiteSpace(species) ||
            string.IsNullOrWhiteSpace(gender) ||
            string.IsNullOrWhiteSpace(size))
        {
            ValidationLabel.Text = "Заполните вид, пол и размер животного.";
            ValidationLabel.IsVisible = true;
            return;
        }

        if (AnimalStore.Animals.Any(a =>
                a.Number.Equals(number, StringComparison.OrdinalIgnoreCase)))
        {
            ValidationLabel.Text = "Животное с таким идентификационным номером уже зарегистрировано.";
            ValidationLabel.IsVisible = true;
            return;
        }

        AnimalStore.Animals.Add(
            new AnimalItem(
                name,
                number,
                species,
                string.IsNullOrWhiteSpace(breed) ? "Не указана" : breed,
                gender,
                string.IsNullOrWhiteSpace(color) ? "Не указан" : color,
                size,
                string.IsNullOrWhiteSpace(age) ? "Не указан" : age,
                string.IsNullOrWhiteSpace(weight) ? "Не указан" : weight,
                string.IsNullOrWhiteSpace(vaccinations) ? "Не указаны" : vaccinations,
                "В приюте",
                _photo?.FileName ?? "Не прикреплено"));

        await DisplayAlert(
            "Готово",
            $"Животное «{name}» зарегистрировано. Дата приемки: {AdmissionDatePicker.Date:dd.MM.yyyy}.",
            "ОК");

        await Shell.Current.GoToAsync("//ShelterMainPage");
    }

    private async void OnCancelClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//ShelterMainPage");
}