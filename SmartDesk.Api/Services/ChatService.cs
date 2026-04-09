using FluentValidation;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Strategies;

namespace SmartDesk.Api.Services;

/// <summary>
/// Coordinates validation, sentiment analysis, answer generation, and session context for chat interactions.
/// </summary>
public class ChatService : IChatService
{
    private const int MaxContextMessages = 3;

    private readonly ISessionService _sessionService;
    private readonly ISentimentService _sentimentService;
    private readonly AiAnswerStrategy _aiAnswerStrategy;
    private readonly KeywordFallbackStrategy _keywordFallbackStrategy;
    private readonly IValidator<ChatRequest> _requestValidator;
    private readonly IValidator<ChatResponse> _responseValidator;

    public ChatService(
        ISessionService sessionService,
        ISentimentService sentimentService,
        AiAnswerStrategy aiAnswerStrategy,
        KeywordFallbackStrategy keywordFallbackStrategy,
        IValidator<ChatRequest> requestValidator,
        IValidator<ChatResponse> responseValidator)
    {
        _sessionService = sessionService;
        _sentimentService = sentimentService;
        _aiAnswerStrategy = aiAnswerStrategy;
        _keywordFallbackStrategy = keywordFallbackStrategy;
        _requestValidator = requestValidator;
        _responseValidator = responseValidator;
    }

    public async Task<ChatResponse> AskAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestValidation = await _requestValidator.ValidateAsync(request, cancellationToken);
        if (!requestValidation.IsValid)
        {
            throw new ValidationException(requestValidation.Errors);
        }

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
        var usingFallback = false;
        var aiFailureReason = AiServiceFailureReason.None;
        var systemStatusMessage = string.Empty;
        if (!answerResult.Success)
        {
            usingFallback = true;
            aiFailureReason = answerResult.FailureReason;
            answerResult = await _keywordFallbackStrategy.TryGetAnswerAsync(
                request.Message,
                recentContext,
                cancellationToken);

            systemStatusMessage = aiFailureReason switch
            {
                AiServiceFailureReason.QuotaExceeded => "Gemini quota or rate limit was reached. Switched to manual mode.",
                AiServiceFailureReason.RateLimited => "Gemini rate limit was reached. Switched to manual mode.",
                AiServiceFailureReason.MissingApiKey => "AI service is not configured. Switched to manual mode.",
                AiServiceFailureReason.InvalidApiKey => "AI authentication failed (invalid API key). Switched to manual mode.",
                _ => "AI service is unavailable right now. Switched to manual mode."
            };
        }

        var finalAnswer = answerResult.Answer;

        if (priorityEscalation)
        {
            var priorityMessage = "⚠️ Priority Support: We're sorry you're facing issues. Our team will assist you immediately. ";
            finalAnswer = $"{priorityMessage}{finalAnswer}".Trim();
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
            ManualMode = usingFallback,
            SystemStatusMessage = systemStatusMessage,
            Context = _sessionService.GetRecentMessages(sessionId, MaxContextMessages).ToList()
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