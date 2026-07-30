using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetCompany;

public sealed record GetCompanyQuery(Guid CompanyId) : IRequest<Result<DocuGuardAI.Application.Features.Companies.CompanyDto>>;