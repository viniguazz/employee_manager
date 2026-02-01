namespace EmployeeManager.Api.Contracts.Employees;

public sealed record EmployeeLookupResponse(
    Guid Id,
    string Name,
    string Email
);
