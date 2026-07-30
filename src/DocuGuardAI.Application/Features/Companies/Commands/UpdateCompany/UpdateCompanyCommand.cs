using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.UpdateCompany;

public sealed record UpdateCompanyCommand(Guid CallerId, Guid CompanyId, string Name, DocuGuardAI.Domain.Entities.CompanyAdminType AdministratorType) : IRequest<Result<DocuGuardAI.Application.Features.Companies.CompanyDto>>;