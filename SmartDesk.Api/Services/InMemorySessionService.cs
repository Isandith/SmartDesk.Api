using System.Collections.Concurrent;
using SmartDesk.Api.Models.Chat;

namespace SmartDesk.Api.Services;

/// <summary>
/// Stores chat session state in memory for the current application process.
/// </summary>
public class InMemorySessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _sessions = new();

    public string GetOrCreateSessionId(string? requestedSessionId)
    {
        var sessionId = string.IsNullOrWhiteSpace(requestedSessionId)
            ? Guid.NewGuid().ToString("N")
            : requestedSessionId.Trim();

        _sessions.TryAdd(sessionId, new List<ChatMessage>());
        return sessionId;
    }

    public void AddMessage(string sessionId, ChatMessage message)
    {
        var messages = _sessions.GetOrAdd(sessionId, _ => new List<ChatMessage>());

        lock (messages)
        {
            messages.Add(message);

            // Keep history reasonably small.
            while (messages.Count > 20)
            {
                messages.RemoveAt(0);
            }
        }
    }

    public IReadOnlyList<ChatMessage> GetRecentMessages(string sessionId, int take = 3)
    {
        if (!_sessions.TryGetValue(sessionId, out var messages))
        {
            return new List<ChatMessage>();
        }

        lock (messages)
        {
            return messages
                .TakeLast(take)
                .Select(message => new ChatMessage
                {
                    Role = message.Role,
                    Content = message.Content,
                    TimestampUtc = message.TimestampUtc
                })
                .ToList();
        }
    }

    public bool ResetSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }
}