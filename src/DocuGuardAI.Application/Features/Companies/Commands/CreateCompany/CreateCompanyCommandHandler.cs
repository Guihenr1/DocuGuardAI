using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DocuGuardAI.Application.Features.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler(
    ICompanyRepository companyRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateCompanyCommand, Result<CreateCompanyResponse>>
{
    public async Task<Result<CreateCompanyResponse>> Handle(CreateCompanyCommand request, CancellationToken ct)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var callerId))
            return Result.Unauthorized();

        var caller = await userRepository.GetByIdAsync(callerId, ct);
        if (caller == null || caller.Role != UserRole.SystemAdmin)
            return Result.Unauthorized();

        var existing = await companyRepository.GetByNameAsync(request.Name, ct);
        switch (existing)
        {
            case { IsActive: true }:
                return Result.Conflict("Company name already exists");
            case null:
            {
                var company = Company.Create(request.Name, null, request.AdminType);
                await companyRepository.AddAsync(company, ct);

                var admin = User.Create(Email.From(request.AdminEmail), passwordHasher.HashPassword(request.AdminPassword), company.Id, UserRole.Admin);
                await userRepository.AddAsync(admin, ct);

                company.AdministratorId = admin.Id;
                await companyRepository.UpdateAsync(company, ct);

                return Result.Success(new CreateCompanyResponse(company.Id, admin.Id));
            }
        }

        existing.IsActive = true;
            
        await companyRepository.UpdateAsync(existing, ct);

        return Result.Success(new CreateCompanyResponse(existing.Id, existing.AdministratorId!.Value));
    }
}