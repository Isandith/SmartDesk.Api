namespace SmartDesk.Api.Services;

public record SentimentSampleCase(
    string Message,
    double MinExpectedScore,
    double MaxExpectedScore,
    bool ExpectedPriorityEscalation);

public static class SentimentSampleCases
{
    public static IReadOnlyList<SentimentSampleCase> Cases { get; } =
    [
        new("this system is terrible", -1.0, -0.60, true),
        new("not working at all", -1.0, -0.60, true),
        new("What services do you offer?", -0.10, 0.10, false),
        new("great service, thank you", 0.50, 1.0, false),
        new("i really like this system", 0.30, 1.0, false)
    ];
}
