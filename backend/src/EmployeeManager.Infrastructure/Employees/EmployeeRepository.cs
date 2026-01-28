using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Infrastructure.Data;
using EmployeeManager.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Infrastructure.Employees;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _db;

    public EmployeeRepository(AppDbContext db) => _db = db;

    public Task<bool> DocNumberExistsAsync(string docNumber, CancellationToken ct)
        => _db.Employees.AnyAsync(e => e.DocNumber == docNumber, ct);

    public async Task AddAsync(Employee employee, CancellationToken ct)
    {
        var entity = ToEntity(employee);
        _db.Employees.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.Employees
            .Include(e => e.Phones)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<List<Employee>> ListAsync(int skip, int take, CancellationToken ct)
    {
        var entities = await _db.Employees
            .Include(e => e.Phones)
            .OrderBy(e => e.FirstName)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    private static EmployeeEntity ToEntity(Employee e)
    {
        var entity = new EmployeeEntity
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            DocNumber = e.DocNumber,
            BirthDate = e.BirthDate,
            Role = e.Role,
            ManagerEmployeeId = e.ManagerEmployeeId,
            ManagerName = e.ManagerName,
            PasswordHash = e.PasswordHash,
            Phones = e.Phones.Select(p => new EmployeePhoneEntity
            {
                Id = Guid.NewGuid(),
                Number = p.Number,
                Type = p.Type
            }).ToList()
        };

        foreach (var p in entity.Phones)
            p.EmployeeId = entity.Id;

        return entity;
    }

    private static Employee ToDomain(EmployeeEntity e)
    {
        var phones = e.Phones.Select(p => new Phone(p.Number, p.Type)).ToList();

        return Employee.Rehydrate(
            e.Id,
            e.FirstName, e.LastName, e.Email, e.DocNumber,
            e.BirthDate, e.Role,
            phones,
            e.PasswordHash,
            e.ManagerEmployeeId,
            e.ManagerName
        );
    }
}