using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Api.Contracts.Employees;

public sealed record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string DocNumber,
    DateOnly BirthDate,
    Role Role,
    List<CreateEmployeePhoneRequest> Phones,
    string Password,
    Guid? ManagerEmployeeId
);

public sealed record CreateEmployeePhoneRequest(
    string Number,
    string? Type
);