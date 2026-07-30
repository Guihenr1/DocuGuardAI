using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string email, string password) : IRequest<Result<LoginResponse>>;

public sealed record LoginResponse(string AccessToken, Guid UserId, DateTime ExpiresAt, string RefreshToken);