using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies;

public sealed class GetAllCompaniesQueryHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetAllCompaniesQuery, Result<IEnumerable<CompanyDto>>>
{
    public async Task<Result<IEnumerable<CompanyDto>>> Handle(GetAllCompaniesQuery request, CancellationToken ct)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerId))
            return Result<IEnumerable<CompanyDto>>.Unauthorized();

        var caller = await userRepository.GetByIdAsync(callerId, ct);
        if (caller == null || caller.Role != Domain.ValueObjects.UserRole.SystemAdmin)
            return Result<IEnumerable<CompanyDto>>.Unauthorized();

        var companies = await companyRepository.GetAllAsync(ct);
        var dtos = companies.Select(c => new CompanyDto(c.Id, c.Name, c.AdministratorId, c.AdministratorType, c.CreatedAt, c.IsActive));
        return Result.Success(dtos);
    }
}