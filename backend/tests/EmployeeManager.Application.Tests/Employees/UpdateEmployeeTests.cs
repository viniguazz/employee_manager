using EmployeeManager.Application.Employees;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Application.Security;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EmployeeManager.Application.Tests.Employees;

public sealed class UpdateEmployeeTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldUpdatePassword_WhenProvided()
    {
        var existing = BuildEmployee(passwordHash: "OLD_HASH");
        var repo = new RecordingRepo(existing);
        var hasher = new FakeHasher("NEW_HASH");
        var useCase = new UpdateEmployee(repo, hasher, NullLogger<UpdateEmployee>.Instance);
        var updaterId = Guid.NewGuid();

        await useCase.ExecuteAsync(new UpdateEmployeeCommand(
            Id: existing.Id,
            FirstName: "Ana",
            LastName: "Silva",
            Email: "ana@ex.com",
            DocNumber: "12345678901",
            BirthDate: new DateOnly(1990, 1, 1),
            Role: Role.Employee,
            Phones: new List<(string Number, string? Type)>
            {
                ("999991111", "mobile"),
                ("333322222", "home")
            },
            ManagerEmployeeId: null,
            Password: "Newpass1!",
            UpdatedById: updaterId
        ), CancellationToken.None);

        Assert.NotNull(repo.UpdatedEmployee);
        Assert.Equal("NEW_HASH", repo.UpdatedEmployee!.PasswordHash);
        Assert.Equal(updaterId, repo.UpdatedEmployee!.UpdatedById);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldKeepPassword_WhenEmpty()
    {
        var existing = BuildEmployee(passwordHash: "OLD_HASH");
        var repo = new RecordingRepo(existing);
        var hasher = new FakeHasher("NEW_HASH");
        var useCase = new UpdateEmployee(repo, hasher, NullLogger<UpdateEmployee>.Instance);
        var updaterId = Guid.NewGuid();

        await useCase.ExecuteAsync(new UpdateEmployeeCommand(
            Id: existing.Id,
            FirstName: "Ana",
            LastName: "Silva",
            Email: "ana@ex.com",
            DocNumber: "12345678901",
            BirthDate: new DateOnly(1990, 1, 1),
            Role: Role.Employee,
            Phones: new List<(string Number, string? Type)>
            {
                ("999991111", "mobile"),
                ("333322222", "home")
            },
            ManagerEmployeeId: null,
            Password: null,
            UpdatedById: updaterId
        ), CancellationToken.None);

        Assert.NotNull(repo.UpdatedEmployee);
        Assert.Equal("OLD_HASH", repo.UpdatedEmployee!.PasswordHash);
        Assert.Equal(updaterId, repo.UpdatedEmployee!.UpdatedById);
    }

    private static Employee BuildEmployee(string passwordHash)
    {
        var phones = new[]
        {
            new Phone("999991111", "mobile"),
            new Phone("333322222", "home")
        };

        return Employee.Rehydrate(
            id: Guid.NewGuid(),
            firstName: "Ana",
            lastName: "Silva",
            email: "ana@ex.com",
            docNumber: "12345678901",
            birthDate: new DateOnly(1990, 1, 1),
            role: Role.Employee,
            phones: phones,
            passwordHash: passwordHash,
            managerEmployeeId: null,
            createdAt: DateTime.UtcNow.AddDays(-1),
            updatedAt: DateTime.UtcNow.AddDays(-1),
            isActive: true,
            deactivatedAt: null,
            createdById: null,
            updatedById: null,
            inactivatedById: null);
    }

    private sealed class RecordingRepo : IEmployeeRepository
    {
        private readonly Employee _existing;
        public Employee? UpdatedEmployee { get; private set; }

        public RecordingRepo(Employee existing) => _existing = existing;

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct)
            => Task.FromResult(_existing.Id == id ? _existing : null);

        public Task<bool> EmailExistsAsync(string email, Guid? excludeEmployeeId, CancellationToken ct)
            => Task.FromResult(false);

        public Task<bool> DocNumberExistsAsync(string docNumber, CancellationToken ct)
            => Task.FromResult(false);

        public Task<bool> PhoneNumbersExistAsync(IEnumerable<string> numbers, Guid? excludeEmployeeId, CancellationToken ct)
            => Task.FromResult(false);

        public Task UpdateAsync(Employee employee, CancellationToken ct)
        {
            UpdatedEmployee = employee;
            return Task.CompletedTask;
        }

        public Task AddAsync(Employee employee, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<List<Employee>> ListAsync(int skip, int take, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<List<Employee>> ListByManagerAsync(Guid managerId, int skip, int take, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<List<Employee>> SearchAsync(string query, int take, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<List<Employee>> SearchByManagerAsync(Guid managerId, string query, int take, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<bool> IsManagedByAsync(Guid employeeId, Guid managerId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<bool> RemoveAsync(Guid id, Guid? inactivatedById, CancellationToken ct)
            => throw new NotImplementedException();
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        private readonly string _hash;
        public FakeHasher(string hash) => _hash = hash;
        public string Hash(string password) => _hash;
        public bool Verify(string password, string passwordHash) => true;
    }
}
