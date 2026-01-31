using EmployeeManager.Application.Security;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManager.Infrastructure.Security;

public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string passwordHash)
        => _hasher.VerifyHashedPassword(new object(), passwordHash, password)
           == PasswordVerificationResult.Success;
}