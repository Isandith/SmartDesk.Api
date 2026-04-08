using Microsoft.AspNetCore.Mvc;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Services;

namespace SmartDesk.Api.Controllers;

/// <summary>
/// Exposes chat endpoints for asking questions and resetting sessions.
/// </summary>
[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// Asks a question and receives a response.
    /// </summary>
    /// <param name="request">The chat request containing the question.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat response.</returns>
    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponse>> Ask(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _chatService.AskAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Resets the chat session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>Outcome of the reset operation.</returns>
    [HttpPost("reset/{sessionId}")]
    public ActionResult<ResetSessionResponse> Reset(string sessionId)
    {
        var cleared = _chatService.ResetSession(sessionId);

        return Ok(new ResetSessionResponse
        {
            SessionId = sessionId,
            Cleared = cleared
        });
    }
}