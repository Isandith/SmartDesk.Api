using System.Text.Json.Serialization;

namespace SmartDesk.Api.Models.Chat;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp_utc")]
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}