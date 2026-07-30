using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.DeleteCompany;

public sealed record DeleteCompanyCommand(Guid CallerId, Guid CompanyId) : IRequest<Result>;