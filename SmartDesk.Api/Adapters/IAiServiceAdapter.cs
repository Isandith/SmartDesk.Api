using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Models.KnowledgeBase;

namespace SmartDesk.Api.Adapters;

public interface IAiServiceAdapter
{
    Task<AiServiceResult> GetAnswerAsync(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        KnowledgeBaseDocument knowledgeBase,
        CancellationToken cancellationToken = default);
}