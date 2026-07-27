using Ardalis.Result;
using DocuGuardAI.Application.Features.Auth.Commands.Login;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(string RefreshToken) 
    : IRequest<Result<LoginResponse>>;