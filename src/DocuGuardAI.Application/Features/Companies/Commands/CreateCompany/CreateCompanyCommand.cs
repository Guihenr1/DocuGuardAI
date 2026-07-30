using Ardalis.Result;
using DocuGuardAI.Domain.Entities;
using FluentValidation;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.CreateCompany;

public sealed record CreateCompanyCommand(
    string Name,
    string AdminEmail,
    string AdminPassword,
    CompanyAdminType AdminType) : IRequest<Result<CreateCompanyResponse>>;

public sealed record CreateCompanyResponse(Guid CompanyId, Guid AdminUserId);

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(c => c.Name).NotNull().NotEmpty().MaximumLength(255);
        RuleFor(c => c.AdminEmail).NotNull().NotEmpty();
        RuleFor(c => c.AdminPassword).NotNull().NotEmpty().MinimumLength(8);
        RuleFor(c => c.AdminType).NotNull().NotEmpty();
    }
}