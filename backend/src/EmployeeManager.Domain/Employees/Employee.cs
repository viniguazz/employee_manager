using System.Net.Mail;
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

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? DeactivatedAt { get; private set; }
    public Guid? CreatedById { get; private set; }
    public Guid? UpdatedById { get; private set; }
    public Guid? InactivatedById { get; private set; }

    public Guid? ManagerEmployeeId { get; private set; }

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
        Guid? createdById = null)
    {
        Id = Guid.NewGuid();
        FirstName = Require(firstName, "First name");
        LastName = Require(lastName, "Last name");
        Email = ValidateEmail(Require(email, "Email")).ToLowerInvariant();
        DocNumber = ValidateDocNumber(Require(docNumber, "Doc number"));

        BirthDate = birthDate;
        EnsureValidBirthDate(birthDate);
        EnsureAdult(birthDate);

        Role = role;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        IsActive = true;
        DeactivatedAt = null;
        CreatedById = createdById;
        UpdatedById = createdById;
        InactivatedById = null;

        var phoneList = phones?.ToList() ?? new List<Phone>();
        if (phoneList.Count < 2)
            throw new ArgumentException("At least two phones are required.");
        _phones.AddRange(phoneList);

        PasswordHash = Require(passwordHash, "Password hash");

        if (managerEmployeeId == Id)
            throw new ArgumentException("Employee cannot be their own manager.");

        ManagerEmployeeId = managerEmployeeId;
    }

    public void UpdateManager(Guid? managerEmployeeId)
    {
        if (managerEmployeeId == Id)
            throw new ArgumentException("Employee cannot be their own manager.");

        ManagerEmployeeId = managerEmployeeId;
    }

    public void Touch(Guid? updatedById = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedById = updatedById ?? UpdatedById;
    }

    public void Deactivate(Guid? inactivatedById = null)
    {
        if (!IsActive) return;
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        UpdatedAt = DeactivatedAt.Value;
        InactivatedById = inactivatedById;
        UpdatedById = inactivatedById ?? UpdatedById;
    }

    private static string Require(string value, string field)
    {
        value = (value ?? "").Trim();
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} is required.");
        return value;
    }

    private static string ValidateEmail(string value)
    {
        try
        {
            var addr = new MailAddress(value);
            if (addr.Address != value)
                throw new ArgumentException("Email is invalid.");
            return value;
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email is invalid.");
        }
    }

    private static string ValidateDocNumber(string value)
    {
        if (!value.All(char.IsDigit))
            throw new ArgumentException("Doc number must contain only digits.");
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

    private static void EnsureValidBirthDate(DateOnly birthDate)
    {
        if (birthDate == DateOnly.MinValue)
            throw new ArgumentException("Birth date is required.");

        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Birth date cannot be in the future.");
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
    DateTime createdAt,
    DateTime updatedAt,
    bool isActive,
    DateTime? deactivatedAt,
    Guid? createdById,
    Guid? updatedById,
    Guid? inactivatedById)
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
            createdById
        );

        e.Id = id;
        e.CreatedAt = createdAt;
        e.UpdatedAt = updatedAt;
        e.IsActive = isActive;
        e.DeactivatedAt = deactivatedAt;
        e.CreatedById = createdById;
        e.UpdatedById = updatedById;
        e.InactivatedById = inactivatedById;
        return e;
    }
}