using System.Text.Json.Serialization;

namespace SmartDesk.Api.Models.Chat;

/// <summary>
/// Represents the incoming API payload for a chat question.
/// </summary>
public class ChatRequest
{
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}