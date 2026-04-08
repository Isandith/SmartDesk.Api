namespace SmartDesk.Api.Models.Chat;

public enum AiServiceFailureReason
{
    None = 0,
    MissingApiKey,
    InvalidApiKey,
    QuotaExceeded,
    RateLimited,
    EmptyResponse,
    ApiError,
    Exception
}

public class AiServiceResult
{
    public bool Success { get; set; }

    public string Answer { get; set; } = string.Empty;

    public AiServiceFailureReason FailureReason { get; set; } = AiServiceFailureReason.None;

    public static AiServiceResult Ok(string answer)
    {
        return new AiServiceResult
        {
            Success = true,
            Answer = answer,
            FailureReason = AiServiceFailureReason.None
        };
    }

    public static AiServiceResult Fail(AiServiceFailureReason failureReason)
    {
        return new AiServiceResult
        {
            Success = false,
            Answer = string.Empty,
            FailureReason = failureReason
        };
    }
}