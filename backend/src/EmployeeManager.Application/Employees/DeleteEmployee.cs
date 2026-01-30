using EmployeeManager.Application.Employees.Ports;

namespace EmployeeManager.Application.Employees;

public sealed class DeleteEmployee
{
    private readonly IEmployeeRepository _repo;

    public DeleteEmployee(IEmployeeRepository repo) => _repo = repo;

    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        var removed = await _repo.RemoveAsync(id, ct);
        if (!removed)
            throw new InvalidOperationException("Employee not found.");
    }
}