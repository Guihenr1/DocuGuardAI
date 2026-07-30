using System.Security.Claims;
using DocuGuardAI.Api.Extensions;
using DocuGuardAI.Application.Features.Auth.Commands.Login;
using DocuGuardAI.Application.Features.Auth.Commands.Logout;
using DocuGuardAI.Application.Features.Auth.Commands.Refresh;
using DocuGuardAI.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuGuardAI.Api.Controllers;

[ApiController]
[Route("auth")]
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await mediator.Send(command);

        return result.ToActionResult(this);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);

        return result.ToActionResult(this);
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await mediator.Send(command);

        return result.ToActionResult(this);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await mediator.Send(new LogoutCommand(userId));

        return result.ToActionResult(this);
    }
}