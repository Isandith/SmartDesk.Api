using SmartDesk.Api.Adapters;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Services;

namespace SmartDesk.Api.Strategies;

public class AiAnswerStrategy
{
    private readonly IAiServiceAdapter _aiServiceAdapter;
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    public AiAnswerStrategy(
        IAiServiceAdapter aiServiceAdapter,
        IKnowledgeBaseService knowledgeBaseService)
    {
        _aiServiceAdapter = aiServiceAdapter;
        _knowledgeBaseService = knowledgeBaseService;
    }

    public async Task<AnswerResult> TryGetAnswerAsync(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        CancellationToken cancellationToken = default)
    {
        var knowledgeBase = await _knowledgeBaseService.GetKnowledgeBaseAsync(cancellationToken);

        var answer = await _aiServiceAdapter.GetAnswerAsync(
            userMessage,
            context,
            knowledgeBase,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(answer))
        {
            return AnswerResult.Fail();
        }

        return AnswerResult.Ok(answer.Trim(), "ai");
    }
}