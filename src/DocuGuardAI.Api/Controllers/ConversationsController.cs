using System.Security.Claims;
using DocuGuardAI.Application.Features.Conversations.Command.CreateConversation;
using DocuGuardAI.Application.Features.Conversations.Command.SendMessage;
using DocuGuardAI.Application.Features.Conversations.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocuGuardAI.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new conversation
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateConversation(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new CreateConversationCommand(userId);
        var result = await mediator.Send(command, ct);

        return Ok(result);
    }

    /// <summary>
    /// Sends a message in an existing conversation
    /// </summary>
    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new SendMessageCommand(
            ConversationId: conversationId,
            UserId: userId,
            Message: request.Message,
            DocumentId: request.DocumentId);

        var result = await mediator.Send(command, ct);

        return Ok(result);
    }
    
    /// <summary>
    /// Gets a conversation with its full message history
    /// </summary>
    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(
        Guid conversationId,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetConversationQuery(conversationId, userId);
        var result = await mediator.Send(query, ct);

        return Ok(result);
    }
}

public record SendMessageRequest(
    string Message,
    Guid? DocumentId = null);