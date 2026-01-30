using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;

namespace EmployeeManager.Application.Employees;

public sealed class GetEmployee
{
    private readonly IEmployeeRepository _repo;

    public GetEmployee(IEmployeeRepository repo) => _repo = repo;

    public Task<Employee?> ExecuteAsync(Guid id, CancellationToken ct)
        => _repo.GetByIdAsync(id, ct);
}