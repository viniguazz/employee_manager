using EmployeeManager.Domain.Employees;

namespace EmployeeManager.Application.Employees.Ports;

public interface IEmployeeRepository
{
    Task<bool> DocNumberExistsAsync(string docNumber, CancellationToken ct);
    Task AddAsync(Employee employee, CancellationToken ct);
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<Employee>> ListAsync(int skip, int take, CancellationToken ct);
}