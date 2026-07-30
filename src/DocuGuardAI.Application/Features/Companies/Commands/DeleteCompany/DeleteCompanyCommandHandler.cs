using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DocuGuardAI.Application.Features.Companies.Commands.DeleteCompany;

public sealed class DeleteCompanyCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<DeleteCompanyCommand, Result>
{
    public async Task<Result> Handle(DeleteCompanyCommand request, CancellationToken ct)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerId))
            return Result.Unauthorized();

        var caller = await userRepository.GetByIdAsync(callerId, ct);
        if (caller == null || caller.Role != DocuGuardAI.Domain.ValueObjects.UserRole.SystemAdmin)
            return Result.Unauthorized();

        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company == null) return Result.NotFound();

        await companyRepository.DeleteAsync(company, ct);
        return Result.Success();
    }
}