using MediatR;

namespace DocuGuardAI.Application.Features.Conversations.Command.CreateConversation;

public record CreateConversationCommand(string UserId) 
    : IRequest<CreateConversationResult>;

public record CreateConversationResult(Guid ConversationId, DateTime CreatedAt);