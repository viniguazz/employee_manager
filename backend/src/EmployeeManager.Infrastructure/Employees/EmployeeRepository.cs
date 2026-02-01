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

    public Task<bool> EmailExistsAsync(string email, Guid? excludeEmployeeId, CancellationToken ct)
    {
        var normalized = email.ToLower();
        return _db.Employees.AnyAsync(e =>
            e.Email.ToLower() == normalized &&
            (!excludeEmployeeId.HasValue || e.Id != excludeEmployeeId.Value), ct);
    }

    public Task<bool> PhoneNumbersExistAsync(IEnumerable<string> numbers, Guid? excludeEmployeeId, CancellationToken ct)
    {
        var list = numbers.ToList();
        return _db.EmployeePhones.AnyAsync(p =>
            list.Contains(p.Number) &&
            (!excludeEmployeeId.HasValue || p.EmployeeId != excludeEmployeeId.Value), ct);
    }

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
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive, ct);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<List<Employee>> ListAsync(int skip, int take, CancellationToken ct)
    {
        var entities = await _db.Employees
            .Include(e => e.Phones)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FirstName)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return entities.Select(ToDomain).ToList();
    }

    public async Task<List<Employee>> SearchAsync(string query, int take, CancellationToken ct)
    {
        var q = query.Trim().ToLower();
        if (string.IsNullOrWhiteSpace(q)) return new List<Employee>();

        var entities = await _db.Employees
            .Include(e => e.Phones)
            .Where(e =>
                e.IsActive && (
                    e.FirstName.ToLower().Contains(q) ||
                    e.LastName.ToLower().Contains(q) ||
                    e.Email.ToLower().Contains(q)))
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
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
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            IsActive = e.IsActive,
            DeactivatedAt = e.DeactivatedAt,
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
            e.ManagerName,
            e.CreatedAt,
            e.UpdatedAt,
            e.IsActive,
            e.DeactivatedAt
        );
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct)
    {
        var existing = await _db.Employees
        .FirstOrDefaultAsync(e => e.Id == employee.Id, ct);


        if (existing is null)
        throw new InvalidOperationException("Employee not found.");

        existing.FirstName = employee.FirstName;
        existing.LastName = employee.LastName;
        existing.Email = employee.Email;
        existing.BirthDate = employee.BirthDate;
        existing.Role = employee.Role;
        existing.CreatedAt = employee.CreatedAt;
        existing.UpdatedAt = employee.UpdatedAt;
        existing.IsActive = employee.IsActive;
        existing.DeactivatedAt = employee.DeactivatedAt;
        existing.ManagerEmployeeId = employee.ManagerEmployeeId;
        existing.ManagerName = employee.ManagerName;

        await _db.EmployeePhones
        .Where(p => p.EmployeeId == employee.Id)
        .ExecuteDeleteAsync(ct);

        var newPhones = employee.Phones.Select(p => new EmployeePhoneEntity
        {
        Id = Guid.NewGuid(),
        EmployeeId = employee.Id,
        Number = p.Number,
        Type = p.Type
        }).ToList();

        _db.EmployeePhones.AddRange(newPhones);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct)
    {
        var existing = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id && e.IsActive, ct);
        if (existing is null) return false;

        existing.IsActive = false;
        existing.DeactivatedAt = DateTime.UtcNow;
        existing.UpdatedAt = existing.DeactivatedAt.Value;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var entity = await _db.Employees
        .Include(e => e.Phones)
        .FirstOrDefaultAsync(e => e.Email == email.ToLower() && e.IsActive, ct);
        return entity is null ? null : ToDomain(entity);
    }

}