using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeManager.Api.Contracts.Employees;
using EmployeeManager.Application.Employees;
using EmployeeManager.Domain.Employees;
using EmployeeManager.Application.Employees.Ports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManager.Domain.Roles;
using Microsoft.Extensions.Logging;

namespace EmployeeManager.Api.Controllers;

[Authorize]
[ApiController]
[Route("employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(ILogger<EmployeesController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromServices] CreateEmployee useCase,
        [FromBody] CreateEmployeeRequest req,
        CancellationToken ct)
    {
        var passwordHash = "TEMP_HASH_" + req.Password;

        var creatorIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (creatorIdStr is null || !Guid.TryParse(creatorIdStr, out var creatorId))
        {
            var claimsDump = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
            _logger.LogWarning("Invalid creator identity. Claims: {Claims}", claimsDump);
            throw new InvalidOperationException("Invalid creator identity.");
        }

        var creatorRoleStr = User.Claims.First(c => c.Type == ClaimTypes.Role).Value;
        var creatorRole = Enum.Parse<Role>(creatorRoleStr);
        _logger.LogInformation("POST /employees by {UserId} role {Role}", creatorId, creatorRole);
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
            creatorId,
            creatorRole
        ), ct);

        return Created($"/employees/{id}", new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        [FromServices] GetEmployee useCase,
        [FromServices] IEmployeeRepository repo,
        Guid id,
        CancellationToken ct)
    {
        var (userId, role) = GetCurrentUser();
        _logger.LogInformation("GET /employees/{EmployeeId} by {UserId} role {Role}", id, userId, role);
        if (role != Role.Director && !(await repo.IsManagedByAsync(id, userId, ct)))
            return NotFound();

        var employee = await useCase.ExecuteAsync(id, ct);
        if (employee is null) return NotFound();

        return Ok(ToResponse(employee));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromServices] ListEmployees useCase,
        [FromServices] IEmployeeRepository repo,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(0, skip);

        var (userId, role) = GetCurrentUser();
        _logger.LogInformation("GET /employees?skip={Skip}&take={Take} by {UserId} role {Role}", skip, take, userId, role);
        var employees = role == Role.Director
            ? await useCase.ExecuteAsync(skip, take, ct)
            : await repo.ListByManagerAsync(userId, skip, take, ct);
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
        var (userId, role) = GetCurrentUser();
        _logger.LogInformation("GET /employees/search?q={Query}&take={Take} by {UserId} role {Role}", q, take, userId, role);
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
            e.ManagerEmployeeId
        );
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromServices] UpdateEmployee useCase,
        [FromServices] IEmployeeRepository repo,
        Guid id,
        [FromBody] UpdateEmployeeRequest req,
        CancellationToken ct)
    {
        var (userId, role) = GetCurrentUser();
        _logger.LogInformation("PUT /employees/{EmployeeId} by {UserId} role {Role}", id, userId, role);
        _logger.LogInformation("Update payload for {EmployeeId}: passwordProvided={PasswordProvided}",
            id, !string.IsNullOrWhiteSpace(req.Password));
        if (role != Role.Director && !(await repo.IsManagedByAsync(id, userId, ct)))
            return NotFound();

        await useCase.ExecuteAsync(new UpdateEmployeeCommand(
            id,
            req.FirstName,
            req.LastName,
            req.Email,
            req.DocNumber,
            req.BirthDate,
            req.Role,
            req.Phones.Select(p => (p.Number, p.Type)).ToList(),
            req.ManagerEmployeeId,
            req.Password,
            userId
        ), ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] DeleteEmployee useCase,
        [FromServices] IEmployeeRepository repo,
        Guid id,
        CancellationToken ct)
    {
        var (userId, role) = GetCurrentUser();
        _logger.LogInformation("DELETE /employees/{EmployeeId} by {UserId} role {Role}", id, userId, role);
        if (role != Role.Director)
        {
            if (id == userId) return Forbid();
            if (!(await repo.IsManagedByAsync(id, userId, ct))) return NotFound();
        }

        await useCase.ExecuteAsync(id, userId, ct);
        return NoContent();
    }

    private (Guid userId, Role role) GetCurrentUser()
    {
        var userIdStr = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
            throw new InvalidOperationException("Invalid creator identity.");

        var roleStr = User.Claims.First(c => c.Type == ClaimTypes.Role).Value;
        var role = Enum.Parse<Role>(roleStr);
        return (userId, role);
    }
}