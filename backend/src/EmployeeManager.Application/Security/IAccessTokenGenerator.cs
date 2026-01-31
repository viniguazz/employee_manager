using EmployeeManager.Domain.Roles;

namespace EmployeeManager.Application.Security;

public interface IAccessTokenGenerator
{
    string Generate(Guid employeeId, string email, Role role);
}