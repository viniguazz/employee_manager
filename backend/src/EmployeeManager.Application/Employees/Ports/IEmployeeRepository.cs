using EmployeeManager.Domain.Employees;

namespace EmployeeManager.Application.Employees.Ports;

public interface IEmployeeRepository
{
    Task<bool> DocNumberExistsAsync(string docNumber, CancellationToken ct);
    Task AddAsync(Employee employee, CancellationToken ct);

    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Employee>> ListAsync(int skip, int take, CancellationToken ct);

    Task UpdateAsync(Employee employee, CancellationToken ct);
    Task<bool> RemoveAsync(Guid id, CancellationToken ct);

    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct);
}