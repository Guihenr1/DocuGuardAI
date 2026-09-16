using Azure.AI.OpenAI;
using Azure.Identity;
using DocuGuardAI.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using ChatMessage = DocuGuardAI.Domain.Entities.ChatMessage;

namespace DocuGuardAI.Infrastructure.Services;

public class AzureOpenAIChatCompletionService : IChatCompletionService
{
    private readonly ChatClient _chatClient;

    public AzureOpenAIChatCompletionService(IOptions<AzureOpenAISettings> settings)
    {
        var config = settings.Value;

        var azureClient = new AzureOpenAIClient(
            new Uri(config.Endpoint),
            new DefaultAzureCredential()); 

        _chatClient = azureClient.GetChatClient(config.DeploymentName);
    }

    public async Task<string> GetResponseAsync(
        IReadOnlyList<ChatMessage> messages,
        string? documentContext = null,
        CancellationToken cancellationToken = default)
    {
        var chatMessages = new List<OpenAI.Chat.ChatMessage>();

        var systemPrompt = "You are a helpful document analysis assistant. " +
                           "Answer based on the provided document context when available.";

        if (!string.IsNullOrWhiteSpace(documentContext))
        {
            systemPrompt += $"\n\nDocument Context:\n{documentContext}";
        }

        chatMessages.Add(new SystemChatMessage(systemPrompt));

        foreach (var message in messages)
        {
            if (message.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                chatMessages.Add(new UserChatMessage(message.Content));
            }
            else if (message.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
            {
                chatMessages.Add(new AssistantChatMessage(message.Content));
            }
        }

        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            chatMessages,
            cancellationToken: cancellationToken);

        return completion.Content[0].Text;
    }
}