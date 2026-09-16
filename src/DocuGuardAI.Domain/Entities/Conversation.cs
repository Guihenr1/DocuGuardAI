namespace DocuGuardAI.Domain.Entities;

public class Conversation
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ChatMessage> _messages = new();
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private Conversation() { }

    public static Conversation Create(string userId)
    {
        return new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddMessage(string role, string content)
    {
        _messages.Add(ChatMessage.Create(role, content));
        UpdatedAt = DateTime.UtcNow;
    }
}