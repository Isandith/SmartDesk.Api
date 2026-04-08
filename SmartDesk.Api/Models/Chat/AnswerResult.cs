namespace SmartDesk.Api.Models.Chat;

/// <summary>
/// Represents the outcome of resolving an answer through AI or fallback strategies.
/// </summary>
public class AnswerResult
{
    /// <summary>
    /// Indicates whether the resolution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The resolved answer, if successful.
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// The source of the resolved answer, if successful.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The reason for failure, if the resolution was not successful.
    /// </summary>
    public AiServiceFailureReason FailureReason { get; set; } = AiServiceFailureReason.None;

    /// <summary>
    /// Creates a successful <see cref="AnswerResult"/> instance.
    /// </summary>
    /// <param name="answer">The resolved answer.</param>
    /// <param name="source">The source of the resolved answer.</param>
    /// <returns>A successful <see cref="AnswerResult"/>.</returns>
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

    /// <summary>
    /// Creates a failed <see cref="AnswerResult"/> instance with no specific reason.
    /// </summary>
    /// <returns>A failed <see cref="AnswerResult"/>.</returns>
    public static AnswerResult Fail()
    {
        return Fail(AiServiceFailureReason.None);
    }

    /// <summary>
    /// Creates a failed <see cref="AnswerResult"/> instance with a specific reason.
    /// </summary>
    /// <param name="failureReason">The reason for failure.</param>
    /// <returns>A failed <see cref="AnswerResult"/>.</returns>
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