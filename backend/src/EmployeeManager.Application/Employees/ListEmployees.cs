using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;

namespace EmployeeManager.Application.Employees;

public sealed class ListEmployees
{
    private readonly IEmployeeRepository _repo;

    public ListEmployees(IEmployeeRepository repo) => _repo = repo;

    public Task<List<Employee>> ExecuteAsync(int skip, int take, CancellationToken ct)
        => _repo.ListAsync(skip, take, ct);
}