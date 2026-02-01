using EmployeeManager.Application.Employees.Ports;
using EmployeeManager.Application.Security;
using Microsoft.Extensions.Logging;

namespace EmployeeManager.Application.Auth;

public sealed record LoginCommand(string Email, string Password);

public sealed class Login
{
    private readonly IEmployeeRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IAccessTokenGenerator _tokens;
    private readonly ILogger<Login> _logger;

    public Login(IEmployeeRepository repo, IPasswordHasher hasher, IAccessTokenGenerator tokens, ILogger<Login> logger)
    {
        _repo = repo;
        _hasher = hasher;
        _tokens = tokens;
        _logger = logger;
    }

    public async Task<string> ExecuteAsync(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var employee = await _repo.GetByEmailAsync(email, ct);

        if (employee is null)
        {
            _logger.LogWarning("Invalid credentials for {Email}: user not found or inactive.", email);
            throw new InvalidOperationException("Invalid credentials.");
        }

        if (!_hasher.Verify(cmd.Password, employee.PasswordHash))
        {
            _logger.LogWarning("Invalid credentials for {Email}: password mismatch.", email);
            throw new InvalidOperationException("Invalid credentials.");
        }

        return _tokens.Generate(employee.Id, employee.Email, employee.Role);
    }
}