using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.ValueObjects;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateCompanyCommand, Result<CreateCompanyResponse>>
{
    public async Task<Result<CreateCompanyResponse>> Handle(CreateCompanyCommand request, CancellationToken ct)
    {
        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null || caller.Role != UserRole.SystemAdmin)
            return Result.Unauthorized();

        var existing = await companyRepository.GetByNameAsync(request.Name, ct);
        if (existing != null)
            return Result.Conflict("Company name already exists");

        var company = Company.Create(request.Name, null, request.AdminType);
        await companyRepository.AddAsync(company, ct);

        var admin = User.Create(Email.From(request.AdminEmail), passwordHasher.HashPassword(request.AdminPassword), company.Id, UserRole.Admin);
        await userRepository.AddAsync(admin, ct);

        company.AdministratorId = admin.Id;
        await companyRepository.UpdateAsync(company, ct);

        return Result.Success(new CreateCompanyResponse(company.Id, admin.Id));
    }
}