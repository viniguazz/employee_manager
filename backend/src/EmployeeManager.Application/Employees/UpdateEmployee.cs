using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Application.Employees;

public sealed record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateOnly BirthDate,
    Role Role,
    List<(string Number, string? Type)> Phones,
    Guid? ManagerEmployeeId,
    string? ManagerName
);

public sealed class UpdateEmployee
{
    private readonly IEmployeeRepository _repo;

    public UpdateEmployee(IEmployeeRepository repo) => _repo = repo;

    public async Task ExecuteAsync(UpdateEmployeeCommand cmd, CancellationToken ct)
    {
        var existing = await _repo.GetByIdAsync(cmd.Id, ct);
        if (existing is null)
            throw new InvalidOperationException("Employee not found.");

        var phones = cmd.Phones.Select(p => new Phone(p.Number, p.Type)).ToList();

        var updated = Employee.Rehydrate(
            existing.Id,
            cmd.FirstName,
            cmd.LastName,
            cmd.Email,
            existing.DocNumber,
            cmd.BirthDate,
            cmd.Role,
            phones,
            existing.PasswordHash,
            cmd.ManagerEmployeeId,
            cmd.ManagerName
        );

        await _repo.UpdateAsync(updated, ct);
    }
}