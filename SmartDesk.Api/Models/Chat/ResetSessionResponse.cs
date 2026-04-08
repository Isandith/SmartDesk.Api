using System.Text.Json.Serialization;

namespace SmartDesk.Api.Models.Chat;

public class ResetSessionResponse
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("cleared")]
    public bool Cleared { get; set; }
}