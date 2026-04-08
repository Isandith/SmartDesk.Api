using System.Text.RegularExpressions;

namespace SmartDesk.Api.Services;

/// <summary>
/// Calculates sentiment scores using phrase and keyword rules.
/// </summary>
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
        ["very frustrated"] = -0.75,
        ["does not work"] = -0.75,
        ["is not working"] = -0.75,
        ["keeps crashing"] = -0.80,
        ["keeps failing"] = -0.78,
        ["super slow"] = -0.65,
        ["very disappointed"] = -0.72,
        ["not helpful at all"] = -0.70,
        ["waste of time"] = -0.78,
        ["still broken"] = -0.75,
        ["completely unusable"] = -0.85,
        ["totally unacceptable"] = -0.85,
        ["very unhappy"] = -0.72,
        ["not satisfied"] = -0.70,
        ["poor service"] = -0.68,
        ["bad service"] = -0.70,
        ["terrible experience"] = -0.85,
        ["really frustrated"] = -0.75,
        ["extremely frustrated"] = -0.82,
        ["not resolved"] = -0.62,
        ["cannot login"] = -0.70,
        ["can't login"] = -0.70,
        ["cannot access"] = -0.68,
        ["can't access"] = -0.68,
        ["payment failed"] = -0.72,
        ["order failed"] = -0.70,
        ["very annoying"] = -0.68,
        ["no response"] = -0.60,
        ["still waiting"] = -0.55,
        ["too expensive"] = -0.48,
        ["not fair"] = -0.50,
        ["very confusing"] = -0.55,
        ["hard to use"] = -0.52,
        ["doesn't make sense"] = -0.55,
        ["does not make sense"] = -0.55,
        ["dont like"] = -0.45,
        ["don't like"] = -0.45,
        ["do not like"] = -0.45,
        ["not like"] = -0.35,
        ["dont like this"] = -0.50,
        ["don't like this"] = -0.50,
        ["do not like this"] = -0.50
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
        ["failing"] = -0.35,
        ["failed"] = -0.35,
        ["failure"] = -0.35,
        ["crash"] = -0.45,
        ["crashing"] = -0.50,
        ["bug"] = -0.28,
        ["bugs"] = -0.30,
        ["annoying"] = -0.42,
        ["annoyed"] = -0.42,
        ["disappointed"] = -0.48,
        ["disappointing"] = -0.48,
        ["upset"] = -0.50,
        ["sad"] = -0.45,
        ["unhappy"] = -0.50,
        ["miserable"] = -0.62,
        ["depressed"] = -0.62,
        ["furious"] = -0.78,
        ["mad"] = -0.50,
        ["irritated"] = -0.45,
        ["irritating"] = -0.45,
        ["ridiculous"] = -0.60,
        ["unacceptable"] = -0.72,
        ["horrible"] = -0.78,
        ["pathetic"] = -0.72,
        ["lousy"] = -0.65,
        ["poor"] = -0.30,
        ["confusing"] = -0.35,
        ["complicated"] = -0.25,
        ["difficult"] = -0.25,
        ["hard"] = -0.20,
        ["delay"] = -0.20,
        ["delayed"] = -0.22,
        ["waiting"] = -0.15,
        ["stuck"] = -0.40,
        ["blocked"] = -0.38,
        ["denied"] = -0.32,
        ["expensive"] = -0.20,
        ["overpriced"] = -0.35,
        ["refund"] = -0.12,
        ["complaint"] = -0.42,
        ["complain"] = -0.42,
        ["worse"] = -0.38,
        ["painful"] = -0.45,
        ["nonsense"] = -0.52,
        ["unresolved"] = -0.40,
        ["offline"] = -0.45,
        ["laggy"] = -0.35,
        ["lag"] = -0.28,
        ["timeout"] = -0.35,
        ["timedout"] = -0.35,
        ["unauthorized"] = -0.32,
        ["forbidden"] = -0.32,
        ["invalid"] = -0.20,
        ["wrong"] = -0.25,
        ["dislike"] = -0.45
    };

    private static readonly Dictionary<string, double> PositivePhrases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["great service"] = 0.55,
        ["thank you"] = 0.30,
        ["works well"] = 0.45,
        ["very helpful"] = 0.45,
        ["really like"] = 0.40,
        ["i like this"] = 0.35,
        ["works perfectly"] = 0.70,
        ["working perfectly"] = 0.70,
        ["very satisfied"] = 0.62,
        ["super helpful"] = 0.58,
        ["much better"] = 0.45,
        ["problem solved"] = 0.62,
        ["issue resolved"] = 0.62,
        ["fast response"] = 0.42,
        ["quick response"] = 0.42,
        ["great support"] = 0.60,
        ["excellent support"] = 0.65,
        ["thank you so much"] = 0.52,
        ["really appreciate"] = 0.55,
        ["very happy"] = 0.60,
        ["totally satisfied"] = 0.62,
        ["highly recommend"] = 0.68,
        ["easy to use"] = 0.42,
        ["love this"] = 0.72,
        ["love it"] = 0.72,
        ["well done"] = 0.58,
        ["great job"] = 0.60,
        ["good job"] = 0.50,
        ["keep it up"] = 0.45,
        ["made my day"] = 0.68,
        ["works like a charm"] = 0.70,
        ["fantastic service"] = 0.72,
        ["excellent experience"] = 0.72,
        ["very responsive"] = 0.48,
        ["really fast"] = 0.45,
        ["customer friendly"] = 0.50,
        ["best support"] = 0.68,
        ["all good"] = 0.38,
        ["looks great"] = 0.42,
        ["sounds good"] = 0.35,
        ["that helped"] = 0.42,
        ["that helps"] = 0.42
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
        ["amazing"] = 0.40,
        ["perfect"] = 0.42,
        ["perfectly"] = 0.42,
        ["fantastic"] = 0.45,
        ["wonderful"] = 0.45,
        ["brilliant"] = 0.42,
        ["superb"] = 0.42,
        ["satisfied"] = 0.35,
        ["happy"] = 0.35,
        ["glad"] = 0.30,
        ["pleased"] = 0.35,
        ["delighted"] = 0.40,
        ["love"] = 0.45,
        ["loved"] = 0.45,
        ["lovely"] = 0.38,
        ["appreciate"] = 0.32,
        ["appreciated"] = 0.32,
        ["impressed"] = 0.40,
        ["impressive"] = 0.40,
        ["resolved"] = 0.32,
        ["fixed"] = 0.35,
        ["fast"] = 0.22,
        ["quick"] = 0.20,
        ["responsive"] = 0.25,
        ["reliable"] = 0.30,
        ["stable"] = 0.25,
        ["smooth"] = 0.22,
        ["clear"] = 0.18,
        ["simple"] = 0.18,
        ["easy"] = 0.20,
        ["friendly"] = 0.22,
        ["best"] = 0.35,
        ["recommend"] = 0.32,
        ["recommended"] = 0.32,
        ["outstanding"] = 0.45,
        ["cool"] = 0.20,
        ["yay"] = 0.28,
        ["grateful"] = 0.35,
        ["cheerful"] = 0.30,
        ["joyful"] = 0.35,
        ["positive"] = 0.25,
        ["success"] = 0.30,
        ["successful"] = 0.30,
        ["value"] = 0.15,
        ["affordable"] = 0.20,
        ["improved"] = 0.25,
        ["improvement"] = 0.22,
        ["pleasant"] = 0.28,
        ["genius"] = 0.35,
        ["legendary"] = 0.35
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