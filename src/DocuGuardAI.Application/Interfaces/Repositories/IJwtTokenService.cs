using DocuGuardAI.Domain.Entities;

namespace DocuGuardAI.Application.Interfaces.Repositories;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}