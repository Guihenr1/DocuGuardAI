using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DocuGuardAI.Application.Features.Companies.Commands.AddCompanyUser;

public sealed class AddCompanyUserCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<AddCompanyUserCommand, Result<AddCompanyUserResponse>>
{
    public async Task<Result<AddCompanyUserResponse>> Handle(AddCompanyUserCommand request, CancellationToken ct)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerId))
            return Result.Unauthorized();

        // only allow creating Editor or Viewer
        if (request.Role != UserRole.Editor && request.Role != UserRole.Viewer)
        {
            var validationError = new ValidationError
            {
                Identifier = "Role",
                ErrorMessage = "Role must be Editor or Viewer",
                Severity = ValidationSeverity.Error
            };

            return Result.Invalid(new List<ValidationError> { validationError });
        }

        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company == null)
            return Result.NotFound();

        var caller = await userRepository.GetByIdAsync(callerId, ct);
        if (caller == null)
            return Result.Unauthorized();

        if (company.AdministratorId != caller.Id)
            return Result.Unauthorized();

        var user = User.Create(Email.From(request.Email), passwordHasher.HashPassword(request.Password), company.Id, request.Role);
        await userRepository.AddAsync(user, ct);

        return Result.Success(new AddCompanyUserResponse(user.Id));
    }
}