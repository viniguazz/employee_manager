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
            new Phone("999991111", "mobile"),
            new Phone("333322222", "home")
        };

        Assert.Throws<ArgumentException>(() =>
        new Employee(
        firstName: "Ana",
        lastName: "Silva",
        email: "ana@ex.com",
        docNumber: "12345678901",
        birthDate: birthDate,
        role: Role.Employee,
        phones: phones,
        passwordHash: "HASH",
        managerEmployeeId: null));
    }

    [Fact]
    public void Create_ShouldThrow_WhenLessThanTwoPhones()
    {
        var birthDate = new DateOnly(1999, 1, 1);
        var phones = new[]
        {
            new Phone("999991111", "mobile"),
        };

        Assert.Throws<ArgumentException>(() =>
        new Employee(
            "Ana",
            "Silva",
            "ana@ex.com",
            "12345678901",
            birthDate,
            Role.Employee,
            phones,
            "HASH",
            null));
    }

    [Fact]
    public void Create_ShouldSucceed_WhenValid()
    {
        var birthDate = new DateOnly(1999, 1, 1);
        var phones = new[]
        {
            new Phone("999991111", "mobile"),
            new Phone("333322222", "home")
        };

        var e = new Employee(
            "Ana",
            "Silva",
            "ana@ex.com",
            "12345678901",
            birthDate,
            Role.Employee,
            phones,
            "HASH",
            null);

        Assert.Equal("Ana", e.FirstName);
        Assert.Equal("Silva", e.LastName);
        Assert.Equal(2, e.Phones.Count);
    }

    [Fact]
    public void Create_ShouldThrow_WhenDocNumberHasNonDigits()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            BuildEmployee(docNumber: "DOC-1"));

        Assert.Contains("Doc number", ex.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenEmailInvalid()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            BuildEmployee(email: "invalid-email"));

        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenBirthDateMissing()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            BuildEmployee(birthDate: DateOnly.MinValue));

        Assert.Contains("Birth date", ex.Message);
    }

    [Fact]
    public void UpdateManager_ShouldThrow_WhenSelfManager()
    {
        var employee = BuildEmployee();
        Assert.Throws<ArgumentException>(() => employee.UpdateManager(employee.Id));
    }

    [Fact]
    public void AuditFields_ShouldBeSet_OnCreateAndUpdate()
    {
        var creatorId = Guid.NewGuid();
        var updaterId = Guid.NewGuid();
        var employee = BuildEmployee(createdById: creatorId);

        Assert.Equal(creatorId, employee.CreatedById);
        Assert.Equal(creatorId, employee.UpdatedById);
        Assert.True(employee.IsActive);
        Assert.Null(employee.InactivatedById);

        employee.Touch(updaterId);
        Assert.Equal(updaterId, employee.UpdatedById);
    }

    [Fact]
    public void Deactivate_ShouldSetInactivatedBy()
    {
        var employee = BuildEmployee();
        var inactivatedById = Guid.NewGuid();

        employee.Deactivate(inactivatedById);

        Assert.False(employee.IsActive);
        Assert.NotNull(employee.DeactivatedAt);
        Assert.Equal(inactivatedById, employee.InactivatedById);
        Assert.Equal(inactivatedById, employee.UpdatedById);
    }

    private static Employee BuildEmployee(
        string firstName = "Ana",
        string lastName = "Silva",
        string email = "ana@ex.com",
        string docNumber = "12345678901",
        DateOnly? birthDate = null,
        Guid? managerEmployeeId = null,
        Guid? createdById = null)
    {
        var phones = new[]
        {
            new Phone("999991111", "mobile"),
            new Phone("333322222", "home")
        };

        return new Employee(
            firstName,
            lastName,
            email,
            docNumber,
            birthDate ?? new DateOnly(1990, 1, 1),
            Role.Employee,
            phones,
            "HASH",
            managerEmployeeId,
            createdById);
    }
}