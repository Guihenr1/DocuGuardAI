using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId) : IRequest<Result>;