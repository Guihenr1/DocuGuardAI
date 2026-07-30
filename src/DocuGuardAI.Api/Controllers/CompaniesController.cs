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

    [HttpGet]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> GetAllCompanies()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerGuid))
            return Unauthorized();

        var query = new DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies.GetAllCompaniesQuery(callerGuid);
        var result = await mediator.Send(query);
        return result.ToActionResult(this);
    }

    [HttpGet("{companyId:guid}")]
    public async Task<IActionResult> GetCompany(Guid companyId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerGuid))
            return Unauthorized();

        var query = new DocuGuardAI.Application.Features.Companies.Queries.GetCompany.GetCompanyQuery(callerGuid, companyId);
        var result = await mediator.Send(query);
        return result.ToActionResult(this);
    }

    [HttpPut("{companyId:guid}")]
    public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] DocuGuardAI.Application.Features.Companies.Commands.UpdateCompany.UpdateCompanyCommand payload)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerGuid))
            return Unauthorized();

        var command = payload with { CallerId = callerGuid, CompanyId = companyId };
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpDelete("{companyId:guid}")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> DeleteCompany(Guid companyId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerGuid))
            return Unauthorized();

        var command = new DocuGuardAI.Application.Features.Companies.Commands.DeleteCompany.DeleteCompanyCommand(callerGuid, companyId);
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }
}