namespace SmartDesk.Api.Models.Chat;

public class AnswerResult
{
    public bool Success { get; set; }

    public string Answer { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public static AnswerResult Ok(string answer, string source)
    {
        return new AnswerResult
        {
            Success = true,
            Answer = answer,
            Source = source
        };
    }

    public static AnswerResult Fail()
    {
        return new AnswerResult
        {
            Success = false,
            Answer = string.Empty,
            Source = string.Empty
        };
    }
}