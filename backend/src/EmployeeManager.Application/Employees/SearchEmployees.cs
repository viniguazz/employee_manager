using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;

namespace EmployeeManager.Application.Employees;

public sealed class SearchEmployees
{
    private readonly IEmployeeRepository _repo;

    public SearchEmployees(IEmployeeRepository repo) => _repo = repo;

    public Task<List<Employee>> ExecuteAsync(string query, int take, CancellationToken ct)
        => _repo.SearchAsync(query, take, ct);
}
