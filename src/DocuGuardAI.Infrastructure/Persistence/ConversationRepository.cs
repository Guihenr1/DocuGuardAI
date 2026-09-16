using DocuGuardAI.Application.Common.Interfaces;
using DocuGuardAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocuGuardAI.Infrastructure.Persistence;

public class ConversationRepository(DocuGuardAIDbContext context) : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var conversation = await context.Conversations
            .Include("_messages")
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return conversation;
    }

    public async Task AddAsync(
        Conversation conversation, 
        CancellationToken cancellationToken = default)
    {
        await context.Conversations.AddAsync(conversation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Conversation conversation, 
        CancellationToken cancellationToken = default)
    {
        context.Conversations.Update(conversation);
        await context.SaveChangesAsync(cancellationToken);
    }
}