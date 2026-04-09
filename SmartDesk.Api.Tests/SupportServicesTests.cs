using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Services;
using SmartDesk.Api.Validators;
using Xunit;

namespace SmartDesk.Api.Tests;

public class SupportServicesTests
{
    [Fact]
    public void SentimentService_IdentifiesFrustratedMessages()
    {
        var service = new RuleBasedSentimentService();

        var score = service.Analyze("This system is terrible and not working!");

        Assert.True(score <= -0.6);
        Assert.Equal(-1, score);
    }

    [Fact]
    public void SessionService_KeepsRecentMessagesAndCanReset()
    {
        var service = new InMemorySessionService();
        var sessionId = service.GetOrCreateSessionId(null);

        for (var index = 1; index <= 5; index++)
        {
            service.AddMessage(sessionId, new ChatMessage
            {
                Role = index % 2 == 0 ? "assistant" : "user",
                Content = $"message-{index}",
                TimestampUtc = DateTime.UtcNow.AddMinutes(index)
            });
        }

        var recentMessages = service.GetRecentMessages(sessionId, 3);

        Assert.Equal(3, recentMessages.Count);
        Assert.Equal("message-3", recentMessages[0].Content);
        Assert.Equal("message-5", recentMessages[2].Content);
        Assert.True(service.ResetSession(sessionId));
        Assert.Empty(service.GetRecentMessages(sessionId));
    }

    [Fact]
    public void ChatRequestValidator_RejectsEmptyMessages()
    {
        var validator = new ChatRequestValidator();

        var result = validator.Validate(new ChatRequest
        {
            SessionId = "session-1",
            Message = string.Empty
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ChatRequest.Message));
    }
}