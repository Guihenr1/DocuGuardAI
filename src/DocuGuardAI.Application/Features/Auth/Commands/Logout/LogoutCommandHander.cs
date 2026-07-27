using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHander(
    IRefreshTokenRepository refreshTokenRepository)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await refreshTokenRepository.RevokeTokenAsync(request.UserId, cancellationToken);
        return Result.Success();
    }
}