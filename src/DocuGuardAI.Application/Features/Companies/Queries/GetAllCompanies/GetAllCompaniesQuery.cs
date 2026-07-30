using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies;

public sealed record GetAllCompaniesQuery(Guid CallerId) : IRequest<Result<IEnumerable<DocuGuardAI.Application.Features.Companies.CompanyDto>>>;