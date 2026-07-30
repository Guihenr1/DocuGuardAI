using Ardalis.Result;
using DocuGuardAI.Application.Features.Auth.Commands.Login;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshRepo,
    IUserRepository userRepo,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var oldToken = await refreshRepo.GetByTokenAsync(request.RefreshToken, ct);
        if (oldToken == null || !oldToken.IsValid())
            return Result.Unauthorized("Invalid or expired refresh token.");

        var user = await userRepo.GetByIdAsync(oldToken.UserId, ct);
        if (user == null) return Result.Unauthorized();

        oldToken.Revoke();
        await refreshRepo.UpdateAsync(oldToken, ct);

        var accessToken = jwtTokenService.GenerateToken(user);
        var newRefresh = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(30));
        await refreshRepo.AddAsync(newRefresh, ct);
        
        return Result.Success(new LoginResponse(accessToken, user.Id, DateTime.UtcNow.AddHours(1), newRefresh.Token));
    }
}