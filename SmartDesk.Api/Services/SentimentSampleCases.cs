namespace SmartDesk.Api.Services;

/// <summary>
/// Represents an expected sentiment scoring scenario for test-like validation.
/// </summary>
public record SentimentSampleCase(
    string Message,
    double MinExpectedScore,
    double MaxExpectedScore,
    bool ExpectedPriorityEscalation);

/// <summary>
/// Provides predefined sentiment sample cases for verification and demos.
/// </summary>
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
