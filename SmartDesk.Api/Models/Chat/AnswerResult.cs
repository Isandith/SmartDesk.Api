namespace SmartDesk.Api.Models.Chat;

public class AnswerResult
{
    public bool Success { get; set; }

    public string Answer { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public AiServiceFailureReason FailureReason { get; set; } = AiServiceFailureReason.None;

    public static AnswerResult Ok(string answer, string source)
    {
        return new AnswerResult
        {
            Success = true,
            Answer = answer,
            Source = source,
            FailureReason = AiServiceFailureReason.None
        };
    }

    public static AnswerResult Fail()
    {
        return Fail(AiServiceFailureReason.None);
    }

    public static AnswerResult Fail(AiServiceFailureReason failureReason)
    {
        return new AnswerResult
        {
            Success = false,
            Answer = string.Empty,
            Source = string.Empty,
            FailureReason = failureReason
        };
    }
}