using FluentValidation;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Strategies;

namespace SmartDesk.Api.Services;

public class ChatService : IChatService
{
    private readonly ISessionService _sessionService;
    private readonly ISentimentService _sentimentService;
    private readonly AiAnswerStrategy _aiAnswerStrategy;
    private readonly KeywordFallbackStrategy _keywordFallbackStrategy;
    private readonly IValidator<ChatResponse> _responseValidator;

    public ChatService(
        ISessionService sessionService,
        ISentimentService sentimentService,
        AiAnswerStrategy aiAnswerStrategy,
        KeywordFallbackStrategy keywordFallbackStrategy,
        IValidator<ChatResponse> responseValidator)
    {
        _sessionService = sessionService;
        _sentimentService = sentimentService;
        _aiAnswerStrategy = aiAnswerStrategy;
        _keywordFallbackStrategy = keywordFallbackStrategy;
        _responseValidator = responseValidator;
    }

    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var sessionId = _sessionService.GetOrCreateSessionId(request.SessionId);

        var userMessage = new ChatMessage
        {
            Role = "user",
            Content = request.Message.Trim(),
            TimestampUtc = DateTime.UtcNow
        };

        _sessionService.AddMessage(sessionId, userMessage);

        var recentContext = _sessionService.GetRecentMessages(sessionId, 3);
        var sentimentScore = _sentimentService.Analyze(request.Message);
        var priorityEscalation = sentimentScore < -0.6;

        // Try AI first.
        var answerResult = await _aiAnswerStrategy.TryGetAnswerAsync(
            request.Message,
            recentContext,
            cancellationToken);

        // Fall back to keyword matching if AI fails.
        if (!answerResult.Success)
        {
            answerResult = await _keywordFallbackStrategy.TryGetAnswerAsync(
                request.Message,
                recentContext,
                cancellationToken);
        }

        var finalAnswer = answerResult.Answer;

        if (priorityEscalation)
        {
            finalAnswer =
                "⚠️ Priority Support: We're sorry you're facing issues. Our team will assist you immediately. " +
                finalAnswer;
        }

        var assistantMessage = new ChatMessage
        {
            Role = "assistant",
            Content = finalAnswer,
            TimestampUtc = DateTime.UtcNow
        };

        _sessionService.AddMessage(sessionId, assistantMessage);

        var response = new ChatResponse
        {
            SessionId = sessionId,
            UserMessage = request.Message.Trim(),
            Answer = finalAnswer,
            SentimentScore = sentimentScore,
            PriorityEscalation = priorityEscalation,
            ResponseSource = answerResult.Source,
            Context = _sessionService.GetRecentMessages(sessionId, 3).ToList()
        };

        var validationResult = await _responseValidator.ValidateAsync(response, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return response;
    }

    public bool ResetSession(string sessionId)
    {
        return _sessionService.ResetSession(sessionId);
    }
}