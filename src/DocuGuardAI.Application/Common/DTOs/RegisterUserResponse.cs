namespace DocuGuardAI.Application.Common.DTOs;

public sealed record RegisterUserResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);