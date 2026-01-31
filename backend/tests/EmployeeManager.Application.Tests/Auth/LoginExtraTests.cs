using EmployeeManager.Application.Auth;
using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Application.Security;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;
using Xunit;

namespace EmployeeManager.Application.Tests.Auth;

public sealed class LoginExtraTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldNormalizeEmail_BeforeRepositoryLookup()
    {
        var employee = BuildEmployee(
            id: Guid.NewGuid(),
            email: "director@local.dev",
            hash: "HASH",
            role: Role.Director);

        var repo = new RecordingRepo(employee);
        var hasher = new FakeHasher(ok: true);
        var token = new FakeToken("TOKEN_OK");

        var useCase = new Login(repo, hasher, token);

        var res = await useCase.ExecuteAsync(
            new LoginCommand("  Director@Local.Dev  ", "Admin#123"),
            CancellationToken.None);

        Assert.Equal("TOKEN_OK", res);
        Assert.Equal("director@local.dev", repo.LastEmail);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassEmployeeData_ToTokenGenerator()
    {
        var employeeId = Guid.NewGuid();
        var employee = BuildEmployee(
            id: employeeId,
            email: "director@local.dev",
            hash: "HASH",
            role: Role.Director);

        var repo = new RecordingRepo(employee);
        var hasher = new FakeHasher(ok: true);
        var token = new RecordingToken("TOKEN_OK");

        var useCase = new Login(repo, hasher, token);

        var res = await useCase.ExecuteAsync(
            new LoginCommand("director@local.dev", "Admin#123"),
            CancellationToken.None);

        Assert.Equal("TOKEN_OK", res);
        Assert.Equal(employeeId, token.LastEmployeeId);
        Assert.Equal("director@local.dev", token.LastEmail);
        Assert.Equal(Role.Director, token.LastRole);
    }

    private static Employee BuildEmployee(Guid id, string email, string hash, Role role)
    {
        var phones = new[]
        {
            new Phone("+55 48 99999-1111", "mobile"),
            new Phone("+55 48 3333-2222", "home")
        };

        return Employee.Rehydrate(
            id: id,
            firstName: "Seed",
            lastName: "Director",
            email: email,
            docNumber: "DOC",
            birthDate: new DateOnly(1990, 1, 1),
            role: role,
            phones: phones,
            passwordHash: hash,
            managerEmployeeId: null,
            managerName: null);
    }

    private sealed class RecordingRepo : IEmployeeRepository
    {
        private readonly Employee? _employee;
        public RecordingRepo(Employee? employee) => _employee = employee;

        public string? LastEmail { get; private set; }

        public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct)
        {
            LastEmail = email;
            return Task.FromResult(_employee is not null && _employee.Email == email ? _employee : null);
        }

        public Task<bool> DocNumberExistsAsync(string docNumber, CancellationToken ct)
            => Task.FromResult(false);

        public Task AddAsync(Employee employee, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<List<Employee>> ListAsync(int skip, int take, CancellationToken ct)
            => throw new NotImplementedException();

        public Task UpdateAsync(Employee employee, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<bool> RemoveAsync(Guid id, CancellationToken ct)
            => throw new NotImplementedException();
    }

    private sealed class FakeHasher : IPasswordHasher
    {
        private readonly bool _ok;
        public FakeHasher(bool ok) => _ok = ok;

        public string Hash(string password) => "HASH";
        public bool Verify(string password, string passwordHash) => _ok;
    }

    private sealed class FakeToken : IAccessTokenGenerator
    {
        private readonly string _token;
        public FakeToken(string token) => _token = token;

        public string Generate(Guid employeeId, string email, Role role) => _token;
    }

    private sealed class RecordingToken : IAccessTokenGenerator
    {
        private readonly string _token;
        public RecordingToken(string token) => _token = token;

        public Guid LastEmployeeId { get; private set; }
        public string? LastEmail { get; private set; }
        public Role LastRole { get; private set; }

        public string Generate(Guid employeeId, string email, Role role)
        {
            LastEmployeeId = employeeId;
            LastEmail = email;
            LastRole = role;
            return _token;
        }
    }
}