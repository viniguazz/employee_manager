using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Domain.Employees;

public sealed class Employee
{
    public Guid Id { get; private set; }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string DocNumber { get; private set; }

    public DateOnly BirthDate { get; private set; }
    public Role Role { get; private set; }

    public Guid? ManagerEmployeeId { get; private set; }
    public string? ManagerName { get; private set; }

    private readonly List<Phone> _phones = new();
    public IReadOnlyCollection<Phone> Phones => _phones.AsReadOnly();

    public string PasswordHash { get; private set; }

    #pragma warning disable CS8618
    private Employee() { }
    #pragma warning restore CS8618

    public Employee(
        string firstName,
        string lastName,
        string email,
        string docNumber,
        DateOnly birthDate,
        Role role,
        IEnumerable<Phone> phones,
        string passwordHash,
        Guid? managerEmployeeId = null,
        string? managerName = null)
    {
        Id = Guid.NewGuid();
        FirstName = Require(firstName, "First name");
        LastName = Require(lastName, "Last name");
        Email = Require(email, "Email").ToLowerInvariant();
        DocNumber = Require(docNumber, "Doc number");

        BirthDate = birthDate;
        EnsureAdult(birthDate);

        Role = role;

        var phoneList = phones?.ToList() ?? new List<Phone>();
        if (phoneList.Count < 2)
            throw new ArgumentException("At least two phones are required.");
        _phones.AddRange(phoneList);

        PasswordHash = Require(passwordHash, "Password hash");

        if (managerEmployeeId is null && !string.IsNullOrWhiteSpace(managerName))
            ManagerName = managerName.Trim();
        else
            ManagerEmployeeId = managerEmployeeId;
    }

    public void UpdateManager(Guid? managerEmployeeId, string? managerName)
    {
        if (managerEmployeeId is null && string.IsNullOrWhiteSpace(managerName))
        {
            ManagerEmployeeId = null;
            ManagerName = null;
            return;
        }

        if (managerEmployeeId is not null)
        {
            ManagerEmployeeId = managerEmployeeId;
            ManagerName = null;
            return;
        }

        ManagerEmployeeId = null;
        ManagerName = managerName!.Trim();
    }

    private static string Require(string value, string field)
    {
        value = (value ?? "").Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} is required.");
        return value;
    }

    private static void EnsureAdult(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;

        if (age < 18)
            throw new ArgumentException("Employee must be an adult (18+).");
    }

    public static Employee Rehydrate(
    Guid id,
    string firstName,
    string lastName,
    string email,
    string docNumber,
    DateOnly birthDate,
    Role role,
    IEnumerable<Phone> phones,
    string passwordHash,
    Guid? managerEmployeeId,
    string? managerName)
    {
        var e = new Employee(
            firstName,
            lastName,
            email,
            docNumber,
            birthDate,
            role,
            phones,
            passwordHash,
            managerEmployeeId,
            managerName
        );

        e.Id = id;
        return e;
    }
}