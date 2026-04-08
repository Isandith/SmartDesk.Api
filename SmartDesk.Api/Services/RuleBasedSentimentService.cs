namespace SmartDesk.Api.Services;

public class RuleBasedSentimentService : ISentimentService
{
    private static readonly Dictionary<string, double> NegativeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["terrible"] = -0.45,
        ["not working"] = -0.55,
        ["broken"] = -0.45,
        ["down"] = -0.40,
        ["slow"] = -0.25,
        ["frustrated"] = -0.40,
        ["bad"] = -0.20,
        ["hate"] = -0.40,
        ["issue"] = -0.10,
        ["problem"] = -0.20,
        ["error"] = -0.20,
        ["awful"] = -0.45
    };

    private static readonly Dictionary<string, double> PositiveWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["great"] = 0.30,
        ["good"] = 0.20,
        ["helpful"] = 0.25,
        ["thanks"] = 0.20,
        ["thank you"] = 0.25,
        ["awesome"] = 0.35,
        ["excellent"] = 0.35,
        ["nice"] = 0.15
    };

    public double Analyze(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return 0.0;
        }

        var text = message.Trim().ToLowerInvariant();
        double score = 0.0;

        foreach (var item in NegativeWords)
        {
            if (text.Contains(item.Key))
            {
                score += item.Value;
            }
        }

        foreach (var item in PositiveWords)
        {
            if (text.Contains(item.Key))
            {
                score += item.Value;
            }
        }

        // Add a little extra intensity if the message includes exclamation marks.
        if (text.Contains("!"))
        {
            score += score < 0 ? -0.05 : 0.05;
        }

        // Clamp to the required range.
        score = Math.Max(-1.0, Math.Min(1.0, score));

        return Math.Round(score, 2);
    }
}