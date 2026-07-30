using Ardalis.Result;
using DocuGuardAI.Domain.Entities;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.CreateCompany;

public sealed record CreateCompanyCommand(
    Guid CallerId,
    string Name,
    string AdminEmail,
    string AdminPassword,
    CompanyAdminType AdminType) : IRequest<Result<CreateCompanyResponse>>;

public sealed record CreateCompanyResponse(Guid CompanyId, Guid AdminUserId);