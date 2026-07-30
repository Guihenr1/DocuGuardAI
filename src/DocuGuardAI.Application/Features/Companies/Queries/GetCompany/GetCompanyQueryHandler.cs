using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Queries.GetCompany;

public sealed class GetCompanyQueryHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository) : IRequestHandler<GetCompanyQuery, Result<DocuGuardAI.Application.Features.Companies.CompanyDto>>
{
    public async Task<Result<DocuGuardAI.Application.Features.Companies.CompanyDto>> Handle(GetCompanyQuery request, CancellationToken ct)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company == null) return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.NotFound();

        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null) return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.Unauthorized();

        var allowed = caller.Role == DocuGuardAI.Domain.ValueObjects.UserRole.SystemAdmin || caller.CompanyId == company.Id || company.AdministratorId == caller.Id;
        if (!allowed) return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.Unauthorized();

        var dto = new DocuGuardAI.Application.Features.Companies.CompanyDto(company.Id, company.Name, company.AdministratorId, company.AdministratorType, company.CreatedAt, company.IsActive);
        return Result.Success(dto);
    }
}