namespace DocuGuardAI.Domain.Entities;

public class ChatMessage
{
    public Guid Id { get; private set; }
    public string Role { get; private set; } = string.Empty; 
    public string Content { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }

    private ChatMessage() { }

    public static ChatMessage Create(string role, string content)
    {
        return new ChatMessage
        {
            Id = Guid.NewGuid(),
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
    }
}