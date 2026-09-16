namespace DocuGuardAI.Application.Common.DTOs;

public record ChatMessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime Timestamp);