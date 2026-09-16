using MediatR;

namespace DocuGuardAI.Application.Features.Conversations.Command.SendMessage;

public record SendMessageCommand(
    Guid ConversationId,
    string UserId,
    string Message,
    Guid? DocumentId = null
) : IRequest<SendMessageResult>;

public record SendMessageResult(
    Guid ConversationId,
    string Response,
    Guid? DocumentId = null
);

