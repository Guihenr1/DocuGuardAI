using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.DeleteCompany;

public sealed record DeleteCompanyCommand(Guid CompanyId) : IRequest<Result>;