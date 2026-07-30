using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.UpdateCompany;

public sealed class UpdateCompanyCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository) : IRequestHandler<UpdateCompanyCommand, Result<DocuGuardAI.Application.Features.Companies.CompanyDto>>
{
    public async Task<Result<DocuGuardAI.Application.Features.Companies.CompanyDto>> Handle(UpdateCompanyCommand request, CancellationToken ct)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company == null) return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.NotFound();

        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null) return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.Unauthorized();

        var isAdmin = caller.Role == DocuGuardAI.Domain.ValueObjects.UserRole.SystemAdmin;
        var isCompanyAdmin = company.AdministratorId == caller.Id;
        if (!isAdmin && !isCompanyAdmin) return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.Unauthorized();

        // ensure name uniqueness if changed
        if (!string.Equals(company.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await companyRepository.GetByNameAsync(request.Name, ct);
            if (existing != null && existing.Id != company.Id)
                return Result<DocuGuardAI.Application.Features.Companies.CompanyDto>.Conflict("Company name already exists");

            company.Name = request.Name;
        }

        company.AdministratorType = request.AdministratorType;

        await companyRepository.UpdateAsync(company, ct);

        var dto = new DocuGuardAI.Application.Features.Companies.CompanyDto(company.Id, company.Name, company.AdministratorId, company.AdministratorType, company.CreatedAt, company.IsActive);
        return Result.Success(dto);
    }
}