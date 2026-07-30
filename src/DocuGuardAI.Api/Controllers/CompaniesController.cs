using System.Security.Claims;
using DocuGuardAI.Api.Extensions;
using DocuGuardAI.Application.Features.Companies.Commands.CreateCompany;
using DocuGuardAI.Application.Features.Companies.Commands.AddCompanyUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuGuardAI.Api.Controllers;

[ApiController]
[Route("companies")]
[Authorize]
public class CompaniesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand payload)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerGuid))
            return Unauthorized();

        // ensure caller id is set in command
        var command = payload with { CallerId = callerGuid };
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpPost("{companyId:guid}/users")]
    public async Task<IActionResult> AddUserToCompany(Guid companyId, [FromBody] AddCompanyUserCommand payload)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerGuid))
            return Unauthorized();

        var command = payload with { CallerId = callerGuid, CompanyId = companyId };
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }
}