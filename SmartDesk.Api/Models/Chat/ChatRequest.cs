using System.Text.Json.Serialization;

namespace SmartDesk.Api.Models.Chat;

public class ChatRequest
{
    [JsonPropertyName("session_id")]
    public string? SessionId { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}