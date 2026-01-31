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

        if (await _repo.DocNumberExistsAsync(cmd.DocNumber.Trim(), ct))
            throw new InvalidOperationException("Doc number already exists.");

        var phones = cmd.Phones.Select(p => new Phone(p.Number, p.Type)).ToList();

        var passwordHash = _hasher.Hash(cmd.Password);

        var employee = new Employee(
            cmd.FirstName, cmd.LastName, cmd.Email, cmd.DocNumber,
            cmd.BirthDate, cmd.Role, phones, passwordHash,
            cmd.ManagerEmployeeId, cmd.ManagerName);

        await _repo.AddAsync(employee, ct);
        return employee.Id;
    }
}