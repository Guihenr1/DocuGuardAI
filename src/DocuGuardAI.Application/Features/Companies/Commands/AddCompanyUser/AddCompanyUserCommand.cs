using Ardalis.Result;
using DocuGuardAI.Domain.ValueObjects;
using MediatR;
using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Features.Companies.Commands.AddCompanyUser;

public sealed record AddCompanyUserCommand(
    Guid CallerId,
    Guid CompanyId,
    string Email,
    string Password,
    UserRole Role) : IRequest<Result<AddCompanyUserResponse>>;

public sealed record AddCompanyUserResponse(Guid UserId);