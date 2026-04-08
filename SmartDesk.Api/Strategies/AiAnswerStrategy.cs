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

        var answerResult = await _aiServiceAdapter.GetAnswerAsync(
            userMessage,
            context,
            knowledgeBase,
            cancellationToken);

        if (!answerResult.Success || string.IsNullOrWhiteSpace(answerResult.Answer))
        {
            return AnswerResult.Fail(answerResult.FailureReason);
        }

        return AnswerResult.Ok(answerResult.Answer.Trim(), "ai");
    }
}