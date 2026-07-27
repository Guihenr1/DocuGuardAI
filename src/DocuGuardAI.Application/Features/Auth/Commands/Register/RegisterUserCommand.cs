using Ardalis.Result;
using DocuGuardAI.Application.Common.DTOs;
using FluentValidation;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password) : IRequest<Result<RegisterUserResponse>>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}