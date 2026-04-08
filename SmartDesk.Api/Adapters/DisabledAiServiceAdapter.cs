using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Models.KnowledgeBase;

namespace SmartDesk.Api.Adapters;

/// <summary>
/// No-op AI adapter that always signals fallback mode.
/// </summary>
public class DisabledAiServiceAdapter : IAiServiceAdapter
{
    public Task<AiServiceResult> GetAnswerAsync(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        KnowledgeBaseDocument knowledgeBase,
        CancellationToken cancellationToken = default)
    {
        // This intentionally returns null so the system uses fallback keyword matching.
        // Later you can replace this with a real OpenAI / Gemini / Hugging Face adapter.
        return Task.FromResult(AiServiceResult.Fail(AiServiceFailureReason.MissingApiKey));
    }
}