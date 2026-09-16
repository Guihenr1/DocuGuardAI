using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Application.Interfaces.Repositories;
using DocuGuardAI.Domain.Entities;
using DocuGuardAI.Domain.Enums;
using DocuGuardAI.SharedKernel.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocuGuardAI.Application.Features.Conversations.Command.SendMessage;

public class SendMessageCommandHandler(
    IConversationRepository conversationRepository,
    IDocumentRepository documentRepository,
    IContentSafetyService contentSafetyService,
    IChatCompletionService chatService,
    ILogger<SendMessageCommandHandler> logger)
    : IRequestHandler<SendMessageCommand, SendMessageResult>
{
    public async Task<SendMessageResult> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Content Safety check on user message
        var isSafe = await contentSafetyService.IsTextSafeAsync(request.Message, cancellationToken);
        if (!isSafe)
        {
            throw new BusinessException("Message failed content safety check.");
        }

        // 2. Load conversation
        var conversation = await conversationRepository.GetByIdAsync(
            request.ConversationId,
            cancellationToken);

        if (conversation is null)
            throw new NotFoundException($"Conversation {request.ConversationId} not found.");

        // 3. Handle Document (if provided)
        string? documentContext = null;

        if (request.DocumentId.HasValue)
        {
            var document = await documentRepository.GetByIdAsync(
                request.DocumentId.Value,
                cancellationToken);

            if (document is null)
                throw new NotFoundException($"Document {request.DocumentId} not found.");

            if (!document.CanBeUsedInConversation())
            {
                throw new BusinessException(
                    $"Document is not ready for conversation. Current status: {document.Status}");
            }

            // First time the user asks something about this document
            if (document.Status == DocumentStatus.SafetyCheckPassed)
            {
                document.MarkProcessing();
                await documentRepository.UpdateAsync(document, cancellationToken);
            }

            documentContext = BuildDocumentContext(document);
        }

        // 4. Add user message to conversation
        conversation.AddMessage("user", request.Message);

        // 5. Call the AI
        // Convert IReadOnlyCollection → IReadOnlyList
        var messages = conversation.Messages.ToList();

        var aiResponse = await chatService.GetResponseAsync(
            messages,
            documentContext,
            cancellationToken);

        // 6. Add AI response to conversation
        conversation.AddMessage("assistant", aiResponse);

        // 7. Save conversation
        await conversationRepository.UpdateAsync(conversation, cancellationToken);

        return new SendMessageResult(
            conversation.Id,
            aiResponse,
            request.DocumentId);
    }

    private static string BuildDocumentContext(Document document)
    {
        return $"""
            Document Name: {document.Name}
            Summary: {document.Summary ?? "No summary available"}
            Status: {document.Status}
            """;
    }
}