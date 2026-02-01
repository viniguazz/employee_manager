using System.IdentityModel.Tokens.Jwt;
using EmployeeManager.Api.Contracts.Employees;
using EmployeeManager.Application.Employees;
using EmployeeManager.Domain.Employees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("employees")]
public sealed class EmployeesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateEmployee useCase,
        [FromServices] ILogger<EmployeesController> logger,
        [FromBody] CreateEmployeeRequest req,
        CancellationToken ct)
    {
        var passwordHash = "TEMP_HASH_" + req.Password;

        var creatorIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (creatorIdStr is null || !Guid.TryParse(creatorIdStr, out var creatorId))
        {
            var claimsDump = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
            logger.LogWarning("Invalid creator identity. Claims: {Claims}", claimsDump);
            throw new InvalidOperationException("Invalid creator identity.");
        }

        var creatorRoleStr = User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
        var creatorRole = Enum.Parse<EmployeeManager.Domain.Roles.Role>(creatorRoleStr);
        var id = await useCase.ExecuteAsync(new CreateEmployeeCommand(
            req.FirstName,
            req.LastName,
            req.Email,
            req.DocNumber,
            req.BirthDate,
            req.Role,
            req.Phones.Select(p => (p.Number, p.Type)).ToList(),
            req.Password,
            req.ManagerEmployeeId ?? creatorId,
            req.ManagerName,
            creatorRole
        ), ct);

        return Created($"/employees/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromServices] GetEmployee useCase,
        Guid id,
        CancellationToken ct)
    {
        var employee = await useCase.ExecuteAsync(id, ct);
        if (employee is null) return NotFound();

        return Ok(ToResponse(employee));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListEmployees useCase,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(0, skip);

        var employees = await useCase.ExecuteAsync(skip, take, ct);
        return Ok(employees.Select(ToResponse));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromServices] SearchEmployees useCase,
        [FromQuery] string q,
        [FromQuery] int take = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Ok(Array.Empty<EmployeeLookupResponse>());

        take = Math.Clamp(take, 1, 20);
        var employees = await useCase.ExecuteAsync(q, take, ct);
        var result = employees.Select(e =>
            new EmployeeLookupResponse(e.Id, $"{e.FirstName} {e.LastName}", e.Email));
        return Ok(result);
    }

    private static EmployeeResponse ToResponse(Employee e)
        => new(
            e.Id,
            e.FirstName,
            e.LastName,
            e.Email,
            e.DocNumber,
            e.BirthDate,
            e.Role,
            e.Phones.Select(p => new EmployeePhoneResponse(p.Number, p.Type)).ToList(),
            e.ManagerEmployeeId,
            e.ManagerName
        );
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromServices] UpdateEmployee useCase,
        Guid id,
        [FromBody] UpdateEmployeeRequest req,
        CancellationToken ct)
    {
        await useCase.ExecuteAsync(new UpdateEmployeeCommand(
            id,
            req.FirstName,
            req.LastName,
            req.Email,
            req.BirthDate,
            req.Role,
            req.Phones.Select(p => (p.Number, p.Type)).ToList(),
            req.ManagerEmployeeId,
            req.ManagerName
        ), ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteEmployee useCase,
        Guid id,
        CancellationToken ct)
    {
        await useCase.ExecuteAsync(id, ct);
        return NoContent();
    }
}