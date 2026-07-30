using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetAllCompanies;

public sealed class GetAllCompaniesQueryHandler(ICompanyRepository companyRepository, IUserRepository userRepository)
    : IRequestHandler<GetAllCompaniesQuery, Result<IEnumerable<CompanyDto>>>
{
    public async Task<Result<IEnumerable<CompanyDto>>> Handle(GetAllCompaniesQuery request, CancellationToken ct)
    {
        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null || caller.Role != Domain.ValueObjects.UserRole.SystemAdmin)
            return Result<IEnumerable<CompanyDto>>.Unauthorized();

        var companies = await companyRepository.GetAllAsync(ct);
        var dtos = companies.Select(c => new CompanyDto(c.Id, c.Name, c.AdministratorId, c.AdministratorType, c.CreatedAt, c.IsActive));
        return Result.Success(dtos);
    }
}