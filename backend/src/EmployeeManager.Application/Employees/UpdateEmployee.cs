using System.Text.RegularExpressions;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Application.Security;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;
using Microsoft.Extensions.Logging;

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
    string? Password,
    Guid? UpdatedById
);

public sealed class UpdateEmployee
{
    private readonly IEmployeeRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<UpdateEmployee> _logger;

    public UpdateEmployee(IEmployeeRepository repo, IPasswordHasher hasher, ILogger<UpdateEmployee> logger)
    {
        _repo = repo;
        _hasher = hasher;
        _logger = logger;
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
        var passwordChanged = false;

        if (!string.IsNullOrWhiteSpace(cmd.Password))
        {
            _logger.LogInformation("Password update requested for employee {EmployeeId}", existing.Id);
            try
            {
                ValidatePassword(cmd.Password);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Password validation failed for employee {EmployeeId}", existing.Id);
                throw;
            }
            passwordHash = _hasher.Hash(cmd.Password);
            passwordChanged = true;
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
            existing.DeactivatedAt,
            existing.CreatedById,
            existing.UpdatedById,
            existing.InactivatedById
        );

        _logger.LogInformation(
            "Employee domain rehydrated {EmployeeId} for update. PasswordChanged={PasswordChanged} UpdatedBy={UpdatedById}",
            updated.Id, passwordChanged, cmd.UpdatedById);

        updated.Touch(cmd.UpdatedById);
        await _repo.UpdateAsync(updated, ct);

        _logger.LogInformation("Employee updated {EmployeeId}", updated.Id);
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