using SmartDesk.Api.Models.Chat;

namespace SmartDesk.Api.Services;

public interface ISessionService
{
    string GetOrCreateSessionId(string? requestedSessionId);
    void AddMessage(string sessionId, ChatMessage message);
    IReadOnlyList<ChatMessage> GetRecentMessages(string sessionId, int take = 3);
    bool ResetSession(string sessionId);
}