using System.Text.RegularExpressions;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Application.Security;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Application.Employees;

public sealed record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateOnly BirthDate,
    Role Role,
    List<(string Number, string? Type)> Phones,
    Guid? ManagerEmployeeId,
    string? Password
);

public sealed class UpdateEmployee
{
    private readonly IEmployeeRepository _repo;
    private readonly IPasswordHasher _hasher;

    public UpdateEmployee(IEmployeeRepository repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task ExecuteAsync(UpdateEmployeeCommand cmd, CancellationToken ct)
    {
        var existing = await _repo.GetByIdAsync(cmd.Id, ct);
        if (existing is null)
            throw new InvalidOperationException("Employee not found.");

        var email = cmd.Email.Trim();
        var docNumber = cmd.DocNumber.Trim();
        var phoneNumbers = cmd.Phones.Select(p => p.Number.Trim()).ToList();

        if (await _repo.EmailExistsAsync(email, existing.Id, ct))
            throw new InvalidOperationException("Email already exists.");

        if (docNumber != existing.DocNumber && await _repo.DocNumberExistsAsync(docNumber, ct))
            throw new InvalidOperationException("Document number already exists.");

        if (await _repo.PhoneNumbersExistAsync(phoneNumbers, existing.Id, ct))
            throw new InvalidOperationException("Phone number already exists.");

        var phones = cmd.Phones.Select(p => new Phone(p.Number.Trim(), p.Type)).ToList();
        var passwordHash = existing.PasswordHash;

        if (!string.IsNullOrWhiteSpace(cmd.Password))
        {
            ValidatePassword(cmd.Password);
            passwordHash = _hasher.Hash(cmd.Password);
        }

        var updated = Employee.Rehydrate(
            existing.Id,
            cmd.FirstName,
            cmd.LastName,
            email,
            docNumber,
            cmd.BirthDate,
            cmd.Role,
            phones,
            passwordHash,
            cmd.ManagerEmployeeId,
            existing.CreatedAt,
            existing.UpdatedAt,
            existing.IsActive,
            existing.DeactivatedAt
        );

        updated.Touch();
        await _repo.UpdateAsync(updated, ct);
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