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
    [Authorize(Roles = "SystemAdminOnly")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand payload)
    {
        var command = payload;
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpPost("{companyId:guid}/users")]
    [Authorize]
    public async Task<IActionResult> AddUserToCompany(Guid companyId, [FromBody] AddCompanyUserCommand payload)
    {
        var command = payload with { CompanyId = companyId };
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpGet]
    [Authorize(Roles = "SystemAdminOnly")]
    public async Task<IActionResult> GetAllCompanies()
    {
        var query = new DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies.GetAllCompaniesQuery();
        var result = await mediator.Send(query);
        return result.ToActionResult(this);
    }

    [HttpGet("{companyId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetCompany(Guid companyId)
    {
        var query = new DocuGuardAI.Application.Features.Companies.Queries.GetCompany.GetCompanyQuery(companyId);
        var result = await mediator.Send(query);
        return result.ToActionResult(this);
    }

    [HttpPut("{companyId:guid}")]
    [Authorize(Roles = "SystemAdminOnly")]
    public async Task<IActionResult> UpdateCompany(Guid companyId, [FromBody] DocuGuardAI.Application.Features.Companies.Commands.UpdateCompany.UpdateCompanyCommand payload)
    {
        var command = payload with { CompanyId = companyId };
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }

    [HttpDelete("{companyId:guid}")]
    [Authorize(Roles = "SystemAdminOnly")]
    public async Task<IActionResult> DeleteCompany(Guid companyId)
    {
        var command = new DocuGuardAI.Application.Features.Companies.Commands.DeleteCompany.DeleteCompanyCommand(companyId);
        var result = await mediator.Send(command);
        return result.ToActionResult(this);
    }
}