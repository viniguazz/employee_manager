using EmployeeManager.Api.Contracts.Employees;
using EmployeeManager.Application.Employees;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("employees")]
public sealed class EmployeesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateEmployee useCase,
        [FromBody] CreateEmployeeRequest req,
        CancellationToken ct)
    {
        
        var passwordHash = "TEMP_HASH_" + req.Password;

        var id = await useCase.ExecuteAsync(new CreateEmployeeCommand(
            req.FirstName,
            req.LastName,
            req.Email,
            req.DocNumber,
            req.BirthDate,
            req.Role,
            req.Phones.Select(p => (p.Number, p.Type)).ToList(),
            passwordHash,
            req.ManagerEmployeeId,
            req.ManagerName
        ), ct);

        return Created($"/employees/{id}", new { id });
    }
}