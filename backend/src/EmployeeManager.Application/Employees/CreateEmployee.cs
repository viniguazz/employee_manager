using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Application.Employees;

public sealed record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateOnly BirthDate,
    Role Role,
    List<(string Number, string? Type)> Phones,
    string PasswordHash,
    Guid? ManagerEmployeeId,
    string? ManagerName);

public sealed class CreateEmployee
{
    private readonly IEmployeeRepository _repo;

    public CreateEmployee(IEmployeeRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> ExecuteAsync(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        if (await _repo.DocNumberExistsAsync(cmd.DocNumber.Trim(), ct))
            throw new InvalidOperationException("Doc number already exists.");

        var phones = cmd.Phones.Select(p => new Phone(p.Number, p.Type)).ToList();

        var employee = new Employee(
            cmd.FirstName, cmd.LastName, cmd.Email, cmd.DocNumber,
            cmd.BirthDate, cmd.Role, phones, cmd.PasswordHash,
            cmd.ManagerEmployeeId, cmd.ManagerName);

        await _repo.AddAsync(employee, ct);
        return employee.Id;
    }
}