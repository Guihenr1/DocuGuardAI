using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Application.Settings;
using DocuGuardAI.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DocuGuardAI.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtSettingsOptions) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettingsOptions.Value;

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roleString = user.Role.ToString();
        if (roleString == "SystemAdmin")
        {
            roleString = "SystemAdminOnly";
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", roleString),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_jwtSettings.AccessTokenMinutes)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}