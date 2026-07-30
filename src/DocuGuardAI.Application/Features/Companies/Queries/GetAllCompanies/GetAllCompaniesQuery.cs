using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies;

public sealed record GetAllCompaniesQuery : IRequest<Result<IEnumerable<DocuGuardAI.Application.Features.Companies.CompanyDto>>>;