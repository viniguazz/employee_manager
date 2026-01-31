using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Application.Security;

namespace EmployeeManager.Application.Auth;

public sealed record LoginCommand(string Email, string Password);

public sealed class Login
{
    private readonly IEmployeeRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IAccessTokenGenerator _tokens;

    public Login(IEmployeeRepository repo, IPasswordHasher hasher, IAccessTokenGenerator tokens)
    {
        _repo = repo;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<string> ExecuteAsync(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var employee = await _repo.GetByEmailAsync(email, ct);

        if (employee is null) throw new InvalidOperationException("Invalid credentials.");

        if (!_hasher.Verify(cmd.Password, employee.PasswordHash))
            throw new InvalidOperationException("Invalid credentials.");

        return _tokens.Generate(employee.Id, employee.Email, employee.Role);
    }
}