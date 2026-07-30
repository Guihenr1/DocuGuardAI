using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies;

public sealed class GetAllCompaniesQueryHandler(ICompanyRepository companyRepository, IUserRepository userRepository)
    : IRequestHandler<GetAllCompaniesQuery, Result<IEnumerable<DocuGuardAI.Application.Features.Companies.CompanyDto>>>
{
    public async Task<Result<IEnumerable<DocuGuardAI.Application.Features.Companies.CompanyDto>>> Handle(GetAllCompaniesQuery request, CancellationToken ct)
    {
        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null || caller.Role != DocuGuardAI.Domain.ValueObjects.UserRole.SystemAdmin)
            return Result<IEnumerable<DocuGuardAI.Application.Features.Companies.CompanyDto>>.Unauthorized();

        var companies = await companyRepository.GetAllAsync(ct);
        var dtos = companies.Select(c => new DocuGuardAI.Application.Features.Companies.CompanyDto(c.Id, c.Name, c.AdministratorId, c.AdministratorType, c.CreatedAt, c.IsActive));
        return Result.Success(dtos);
    }
}