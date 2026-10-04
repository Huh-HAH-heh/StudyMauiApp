namespace StudyMauiApp;

public enum UserRole
{
    Anonim,
    Client,
    Admin,
    Volonteer,
    Veterenar
}

public enum RequestType
{
    Adoption,
    Surrender
}

public sealed class User
{
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public UserRole Role { get; set; }
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
    public string DisplayName => $"{Number} — {Name}";
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
        (SessionObject.IsAdmin || SessionObject.IsVolonteer);
}

public sealed record VeterinaryItem(
    string AnimalName,
    string Procedure,
    DateTime Date)
{
    public string DateText => Date.ToString("dd.MM.yyyy");
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

public static class VeterinaryStore
{
    public static List<VeterinaryItem> Records { get; } =
    [
        new("Белка", "Вакцинация", DateTime.Today),
        new("Барсик", "Осмотр", new DateTime(2026, 10, 1)),
        new("Рекс", "Обработка", new DateTime(2026, 9, 30))
    ];
}