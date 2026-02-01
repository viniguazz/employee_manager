using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Api.Contracts.Employees;

public sealed record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateOnly BirthDate,
    Role Role,
    List<UpdateEmployeePhoneRequest> Phones,
    Guid? ManagerEmployeeId,
    string? Password
);

public sealed record UpdateEmployeePhoneRequest(
    string Number,
    string? Type
);