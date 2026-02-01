using EmployeeManager.Application.Employees.Ports;
using Microsoft.Extensions.Logging;

namespace EmployeeManager.Application.Employees;

public sealed class DeleteEmployee
{
    private readonly IEmployeeRepository _repo;
    private readonly ILogger<DeleteEmployee> _logger;

    public DeleteEmployee(IEmployeeRepository repo, ILogger<DeleteEmployee> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        var removed = await _repo.RemoveAsync(id, ct);
        if (!removed)
        {
            _logger.LogWarning("Employee delete failed. Employee not found {EmployeeId}", id);
            throw new InvalidOperationException("Employee not found.");
        }

        _logger.LogInformation("Employee deactivated {EmployeeId}", id);
    }
}