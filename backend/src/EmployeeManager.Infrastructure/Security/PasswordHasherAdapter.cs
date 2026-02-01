using EmployeeManager.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManager.Infrastructure.Security;

public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string passwordHash)
    {
        var result = _hasher.VerifyHashedPassword(new object(), passwordHash, password);
        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}