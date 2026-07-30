using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Companies.Commands.DeleteCompany;

public sealed class DeleteCompanyCommandHandler(ICompanyRepository companyRepository, IUserRepository userRepository)
    : IRequestHandler<DeleteCompanyCommand, Result>
{
    public async Task<Result> Handle(DeleteCompanyCommand request, CancellationToken ct)
    {
        var caller = await userRepository.GetByIdAsync(request.CallerId, ct);
        if (caller == null || caller.Role != DocuGuardAI.Domain.ValueObjects.UserRole.SystemAdmin)
            return Result.Unauthorized();

        var company = await companyRepository.GetByIdAsync(request.CompanyId, ct);
        if (company == null) return Result.NotFound();

        await companyRepository.DeleteAsync(company, ct);
        return Result.Success();
    }
}