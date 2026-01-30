using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Api.Contracts.Employees;

public sealed record EmployeeResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateOnly BirthDate,
    Role Role,
    List<EmployeePhoneResponse> Phones,
    Guid? ManagerEmployeeId,
    string? ManagerName
);

public sealed record EmployeePhoneResponse(
    string Number,
    string? Type
);