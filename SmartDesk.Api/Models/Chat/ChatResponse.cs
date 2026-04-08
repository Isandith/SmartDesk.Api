using System.Text.Json.Serialization;

namespace SmartDesk.Api.Models.Chat;

public class ChatResponse
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("user_message")]
    public string UserMessage { get; set; } = string.Empty;

    [JsonPropertyName("answer")]
    public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("sentiment_score")]
    public double SentimentScore { get; set; }

    [JsonPropertyName("priority_escalation")]
    public bool PriorityEscalation { get; set; }

    [JsonPropertyName("response_source")]
    public string ResponseSource { get; set; } = string.Empty;

    [JsonPropertyName("manual_mode")]
    public bool ManualMode { get; set; }

    [JsonPropertyName("context")]
    public List<ChatMessage> Context { get; set; } = new();
}