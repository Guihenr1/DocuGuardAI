using Ardalis.Result;
using DocuGuardAI.Application.Common.DTOs;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Application.Settings;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Options;

namespace DocuGuardAI.Application.Features.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettingsOptions)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request, 
        CancellationToken ct)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, ct);
        
        if (existingUser != null)
        {
            return Result.Conflict(
                existingUser.IsActive ? "Email already registered" : "Email already registered, but inactive");
        }
        
        var user = User.Create(
            Email.From(request.Email),
            passwordHasher.HashPassword(request.Password),
            request.CompanyId,
            request.Role
        );

        await userRepository.AddAsync(user, ct);
        
        var accessToken = jwtTokenService.GenerateToken(user);
        
        var refreshToken = RefreshToken.Create(user.Id, DateTime.UtcNow.AddDays(30));
        await refreshTokenRepository.AddAsync(refreshToken, ct);

        return Result.Success(new RegisterUserResponse(
            AccessToken: accessToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes), 
            RefreshToken: refreshToken.Token));
    }
}