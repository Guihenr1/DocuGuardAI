using Ardalis.Result;
using MediatR;

namespace DocuGuardAI.Application.Features.Documents.Commands.Delete;

public record DeleteDocumentCommand(Guid DocumentId, Guid UserId) : IRequest<Result>;