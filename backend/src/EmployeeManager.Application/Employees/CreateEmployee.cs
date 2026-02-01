using System.Text.RegularExpressions;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;
using EmployeeManager.Application.Security;

namespace EmployeeManager.Application.Employees;

public sealed record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateOnly BirthDate,
    Role Role,
    List<(string Number, string? Type)> Phones,
    string Password,
    Guid? ManagerEmployeeId,
    string? ManagerName,
    Role CreatorRole
);

public sealed class CreateEmployee
{
    private readonly IEmployeeRepository _repo;
    private readonly IPasswordHasher _hasher;

    public CreateEmployee(IEmployeeRepository repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<Guid> ExecuteAsync(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        if ((int)cmd.Role > (int)cmd.CreatorRole)
            throw new InvalidOperationException("You cannot create an employee with higher permissions than yours.");

        var email = cmd.Email.Trim();
        var docNumber = cmd.DocNumber.Trim();
        var phoneNumbers = cmd.Phones.Select(p => p.Number.Trim()).ToList();

        if (await _repo.DocNumberExistsAsync(docNumber, ct))
            throw new InvalidOperationException("Document number already exists.");

        if (await _repo.EmailExistsAsync(email, null, ct))
            throw new InvalidOperationException("Email already exists.");

        if (await _repo.PhoneNumbersExistAsync(phoneNumbers, null, ct))
            throw new InvalidOperationException("Phone number already exists.");

        ValidatePassword(cmd.Password);

        var phones = cmd.Phones.Select(p => new Phone(p.Number.Trim(), p.Type)).ToList();

        var passwordHash = _hasher.Hash(cmd.Password);

        var employee = new Employee(
            cmd.FirstName, cmd.LastName, email, docNumber,
            cmd.BirthDate, cmd.Role, phones, passwordHash,
            cmd.ManagerEmployeeId, cmd.ManagerName);

        await _repo.AddAsync(employee, ct);
        return employee.Id;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters.");

        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new InvalidOperationException("Password must contain an uppercase letter.");

        if (!Regex.IsMatch(password, "[a-z]"))
            throw new InvalidOperationException("Password must contain a lowercase letter.");

        if (!Regex.IsMatch(password, "[0-9]"))
            throw new InvalidOperationException("Password must contain a number.");

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            throw new InvalidOperationException("Password must contain a symbol.");
    }
}