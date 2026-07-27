using Ardalis.Result;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Application.Settings;
using DocuGuardAI.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IOptions<JwtSettings> jwtSettingsOptions,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.email, cancellationToken);
        if (user == null || !passwordHasher.VerifyPassword(request.password, user.PasswordHash))
        {
            return Result.Unauthorized("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return Result.Forbidden("User is inactive");
        }

        var accessToken = jwtTokenService.GenerateToken(user);
        
        var refreshToken = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(30));
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        
        return Result.Success(new LoginResponse(
            AccessToken: accessToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes), 
            RefreshToken: refreshToken.Token));
    }
}