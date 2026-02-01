using EmployeeManager.Application.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManager.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] Login useCase, [FromBody] LoginCommand cmd, CancellationToken ct)
    {
        _logger.LogInformation("POST /auth/login for {Email}", cmd.Email?.Trim().ToLowerInvariant());
        var token = await useCase.ExecuteAsync(cmd, ct);
        return Ok(new { accessToken = token });
    }
}