using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetCompany;

public sealed class GetCompanyQueryHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository) : IRequestHandler<GetCompanyQuery, Result<CompanyDto>>
{
    public async Task<Result<CompanyDto>> Handle(GetCompanyQuery request, CancellationToken ct)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company == null) return Result<CompanyDto>.NotFound();

        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null) return Result<CompanyDto>.Unauthorized();

        var allowed = caller.Role == Domain.ValueObjects.UserRole.SystemAdmin || caller.CompanyId == company.Id || company.AdministratorId == caller.Id;
        if (!allowed) return Result<CompanyDto>.Unauthorized();

        var dto = new CompanyDto(company.Id, company.Name, company.AdministratorId, company.AdministratorType, company.CreatedAt, company.IsActive);
        return Result.Success(dto);
    }
}