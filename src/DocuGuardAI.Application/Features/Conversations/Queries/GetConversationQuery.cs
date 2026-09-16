using DocuGuardAI.Application.Common.DTOs;
using MediatR;

namespace DocuGuardAI.Application.Features.Conversations.Queries;

public record GetConversationQuery(Guid ConversationId, string UserId) 
    : IRequest<GetConversationResult>;
    
public record GetConversationResult(
    Guid Id,
    string UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ChatMessageDto> Messages);