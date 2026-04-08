using System.Text.RegularExpressions;

namespace SmartDesk.Api.Services;

public class RuleBasedSentimentService : ISentimentService
{
    private static readonly Dictionary<string, double> NegativePhrases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["system is down"] = -0.90,
        ["not working at all"] = -0.80,
        ["not working"] = -0.75,
        ["very bad"] = -0.70,
        ["really bad"] = -0.70,
        ["extremely slow"] = -0.65,
        ["so slow"] = -0.60,
        ["very frustrated"] = -0.75
    };

    private static readonly Dictionary<string, double> NegativeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["terrible"] = -0.80,
        ["awful"] = -0.78,
        ["worst"] = -0.80,
        ["useless"] = -0.75,
        ["broken"] = -0.72,
        ["frustrated"] = -0.68,
        ["angry"] = -0.70,
        ["down"] = -0.35,
        ["slow"] = -0.25,
        ["bad"] = -0.25,
        ["hate"] = -0.45,
        ["issue"] = -0.10,
        ["problem"] = -0.20,
        ["error"] = -0.20,
        ["fail"] = -0.30,
        ["failing"] = -0.35
    };

    private static readonly Dictionary<string, double> PositivePhrases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["great service"] = 0.55,
        ["thank you"] = 0.30,
        ["works well"] = 0.45,
        ["very helpful"] = 0.45,
        ["really like"] = 0.40,
        ["i like this"] = 0.35
    };

    private static readonly Dictionary<string, double> PositiveWords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["great"] = 0.30,
        ["good"] = 0.20,
        ["helpful"] = 0.25,
        ["thanks"] = 0.25,
        ["awesome"] = 0.35,
        ["excellent"] = 0.35,
        ["nice"] = 0.15,
        ["amazing"] = 0.40
    };

    public double Analyze(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return 0.0;
        }

        string text = Normalize(message);
        string[] words = GetWords(text);

        double score = 0.0;

        // Score full phrases first
        score += SumPhraseMatches(text, NegativePhrases);
        score += SumPhraseMatches(text, PositivePhrases);

        // Score individual words safely
        score += SumWordMatches(words, NegativeWords);
        score += SumWordMatches(words, PositiveWords);

        // Slight boost for emphasis
        if (text.Contains('!'))
        {
            if (score < 0)
            {
                score -= 0.05;
            }
            else if (score > 0)
            {
                score += 0.05;
            }
        }

        score = Math.Clamp(score, -1.0, 1.0);
        return Math.Round(score, 2);
    }

    private static string Normalize(string message)
    {
        return message.Trim().ToLowerInvariant();
    }

    private static string[] GetWords(string text)
    {
        // Extracts only real words, so "bad" won't match "badminton"
        return Regex.Matches(text, @"\b[a-z]+\b")
                    .Select(match => match.Value)
                    .ToArray();
    }

    private static double SumPhraseMatches(string text, Dictionary<string, double> patterns)
    {
        double score = 0.0;

        foreach (var pattern in patterns)
        {
            if (text.Contains(pattern.Key, StringComparison.OrdinalIgnoreCase))
            {
                score += pattern.Value;
            }
        }

        return score;
    }

    private static double SumWordMatches(string[] words, Dictionary<string, double> patterns)
    {
        double score = 0.0;

        foreach (string word in words)
        {
            if (patterns.TryGetValue(word, out double value))
            {
                score += value;
            }
        }

        return score;
    }
}