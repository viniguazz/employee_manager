using EmployeeManager.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok" });

    [HttpGet("db")]
    public async Task<IActionResult> Db([FromServices] AppDbContext db, CancellationToken ct)
    {
        var canConnect = await db.Database.CanConnectAsync(ct);

        if (!canConnect)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { status = "unhealthy", dependency = "postgres" });

        return Ok(new { status = "healthy", dependency = "postgres" });
    }
}