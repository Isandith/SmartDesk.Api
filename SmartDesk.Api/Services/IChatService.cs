using SmartDesk.Api.Models.Chat;

namespace SmartDesk.Api.Services;

public interface IChatService
{
    Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default);
    bool ResetSession(string sessionId);
}