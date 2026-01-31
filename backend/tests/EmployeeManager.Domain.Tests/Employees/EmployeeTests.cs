using EmployeeManager.Domain.Employees;
using EmployeeManager.Domain.Roles;
using Xunit;

namespace EmployeeManager.Domain.Tests.Employees;

public sealed class EmployeeTests
{
    [Fact]
    public void Create_ShouldThrow_WhenMinor()
    {
        var birthDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(-17)); // 17 anos
        var phones = new[]
        {
            new Phone("+55 48 99999-1111", "mobile"),
            new Phone("+55 48 3333-2222", "home")
        };

        Assert.Throws<ArgumentException>(() =>
        new Employee(
        firstName: "Ana",
        lastName: "Silva",
        email: "ana@ex.com",
        docNumber: "DOC-1",
        birthDate: birthDate,
        role: Role.Employee,
        phones: phones,
        passwordHash: "HASH",
        managerEmployeeId: null,
        managerName: null));
    }

    [Fact]
    public void Create_ShouldThrow_WhenLessThanTwoPhones()
    {
        var birthDate = new DateOnly(1999, 1, 1);
        var phones = new[]
        {
            new Phone("+55 48 99999-1111", "mobile"),
        };

        Assert.Throws<ArgumentException>(() =>
        new Employee(
            "Ana",
            "Silva",
            "ana@ex.com",
            "DOC-1",
            birthDate,
            Role.Employee,
            phones,
            "HASH",
            null,
            null));
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValid()
    {
        var birthDate = new DateOnly(1999, 1, 1);
        var phones = new[]
        {
            new Phone("+55 48 99999-1111", "mobile"),
            new Phone("+55 48 3333-2222", "home")
        };

        var e = new Employee(
            "Ana",
            "Silva",
            "ana@ex.com",
            "DOC-1",
            birthDate,
            Role.Employee,
            phones,
            "HASH",
            null,
            null);

        Assert.Equal("Ana", e.FirstName);
        Assert.Equal("Silva", e.LastName);
        Assert.Equal(2, e.Phones.Count);
    }
}