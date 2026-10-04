namespace StudyMauiApp;

public partial class AddAnimalPage : ContentPage
{
    private FileResult? _photo;

    public AddAnimalPage()
    {
        InitializeComponent();

        SpeciesPicker.SelectedIndex = 0;
        GenderPicker.SelectedIndex = 0;
        SizePicker.SelectedIndex = 1;
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
        if (ShelterMainPage.SessionObject.CurrentUser is not { Role: ShelterMainPage.UserRole.Admin or ShelterMainPage.UserRole.Volonteer })
        {
            await DisplayAlert("Нет доступа", "Регистрировать животных могут администратор и волонтер.", "ОК");
            return;
        }

        string name = NameEntry.Text?.Trim() ?? "";
        string number = NumberEntry.Text?.Trim() ?? "";
        string breed = BreedEntry.Text?.Trim() ?? "";
        string color = ColorEntry.Text?.Trim() ?? "Не указан";
        string age = AgeEntry.Text?.Trim() ?? "Не указан";
        string weight = WeightEntry.Text?.Trim() ?? "Не указан";
        string vaccinations = VaccinationsEditor.Text?.Trim() ?? "Не указаны";

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(number) ||
            string.IsNullOrWhiteSpace(breed))
        {
            await DisplayAlert("Не заполнено", "Заполните кличку, идентификационный номер и породу.", "ОК");
            return;
        }

        if (ShelterMainPage.AnimalStore.Animals.Any(a =>
                a.Number.Equals(number, StringComparison.OrdinalIgnoreCase)))
        {
            await DisplayAlert("Ошибка", "Животное с таким идентификационным номером уже есть.", "ОК");
            return;
        }

        string species = SpeciesPicker.SelectedItem?.ToString() ?? "Другое";
        string gender = GenderPicker.SelectedItem?.ToString() ?? "Не указан";
        string size = SizePicker.SelectedItem?.ToString() ?? "Не указан";
        string photo = _photo?.FileName ?? "Не прикреплено";

        ShelterMainPage.AnimalStore.Animals.Add(
            new ShelterMainPage.AnimalItem(
                name,
                number,
                species,
                breed,
                gender,
                color,
                size,
                age,
                weight,
                vaccinations,
                "В приюте",
                photo));

        await DisplayAlert("Готово", $"Животное «{name}» зарегистрировано и добавлено в журнал учета.", "ОК");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnCancelClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}