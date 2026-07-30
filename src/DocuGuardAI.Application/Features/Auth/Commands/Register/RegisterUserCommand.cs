using Ardalis.Result;
using DocuGuardAI.Application.Common.DTOs;
using DocuGuardAI.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    Guid CompanyId,
    UserRole Role) : IRequest<Result<RegisterUserResponse>>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().Must(role => Enum.IsDefined(typeof(UserRole), role))
            .WithMessage("Invalid role value");
    }
}