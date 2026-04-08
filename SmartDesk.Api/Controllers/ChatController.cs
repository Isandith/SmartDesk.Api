using Microsoft.AspNetCore.Mvc;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Services;

namespace SmartDesk.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponse>> Ask(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _chatService.AskAsync(request, cancellationToken);
        return Ok(response);
    }

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