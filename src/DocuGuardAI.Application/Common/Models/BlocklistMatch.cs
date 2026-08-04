namespace DocuGuardAI.Application.Common.Models;

public sealed record BlocklistMatch(
    string BlocklistName,
    string ItemId,
    string Text);