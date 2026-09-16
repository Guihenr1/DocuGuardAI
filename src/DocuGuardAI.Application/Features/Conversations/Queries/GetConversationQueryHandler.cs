using DocuGuardAI.Application.Common.DTOs;
using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.SharedKernel.Exceptions;
using MediatR;

namespace DocuGuardAI.Application.Features.Conversations.Queries;

public class GetConversationQueryHandler(
    IConversationRepository conversationRepository)
    : IRequestHandler<GetConversationQuery, GetConversationResult>
{
    public async Task<GetConversationResult> Handle(
        GetConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId, 
            cancellationToken);

        if (conversation is null)
            throw new NotFoundException($"Conversation {request.ConversationId} not found.");

        if (conversation.UserId != request.UserId)
            throw new BusinessException("You do not have access to this conversation.");

        return new GetConversationResult(
            conversation.Id,
            conversation.UserId,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.Messages
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto(
                    m.Id,
                    m.Role,
                    m.Content,
                    m.Timestamp))
                .ToList());
    }
}