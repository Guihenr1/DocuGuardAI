using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Domain.Entities;
using MediatR;

namespace DocuGuardAI.Application.Features.Conversations.Command.CreateConversation;

public class CreateConversationCommandHandler(
    IConversationRepository conversationRepository)
    : IRequestHandler<CreateConversationCommand, CreateConversationResult>
{
    public async Task<CreateConversationResult> Handle(
        CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var conversation = Conversation.Create(request.UserId);

        await conversationRepository.AddAsync(conversation, cancellationToken);

        return new CreateConversationResult(
            conversation.Id,
            conversation.CreatedAt);
    }
}