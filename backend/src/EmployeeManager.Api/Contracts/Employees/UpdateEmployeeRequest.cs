using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Api.Contracts.Employees;

public sealed record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    DateOnly BirthDate,
    Role Role,
    List<UpdateEmployeePhoneRequest> Phones,
    Guid? ManagerEmployeeId,
    string? ManagerName
);

public sealed record UpdateEmployeePhoneRequest(
    string Number,
    string? Type
);