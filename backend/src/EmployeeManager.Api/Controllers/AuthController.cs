using EmployeeManager.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] Login useCase, [FromBody] LoginCommand cmd, CancellationToken ct)
    {
        var token = await useCase.ExecuteAsync(cmd, ct);
        return Ok(new { accessToken = token });
    }
}