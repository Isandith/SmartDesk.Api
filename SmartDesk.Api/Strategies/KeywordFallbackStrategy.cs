using System.Text.RegularExpressions;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Models.KnowledgeBase;
using SmartDesk.Api.Services;

namespace SmartDesk.Api.Strategies;

public class KeywordFallbackStrategy
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "is", "a", "an", "and", "or", "to", "for", "of", "in",
        "on", "at", "my", "me", "i", "you", "we", "our", "your",
        "how", "what", "do", "does", "can", "could", "would", "should",
        "please", "help", "with", "this", "that", "it", "are", "am"
    };

    public KeywordFallbackStrategy(IKnowledgeBaseService knowledgeBaseService)
    {
        _knowledgeBaseService = knowledgeBaseService;
    }

    public async Task<AnswerResult> TryGetAnswerAsync(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        CancellationToken cancellationToken = default)
    {
        var knowledgeBase = await _knowledgeBaseService.GetKnowledgeBaseAsync(cancellationToken);

        var bestMatch = FindBestMatch(userMessage, knowledgeBase.Faqs);

        if (bestMatch != null)
        {
            return AnswerResult.Ok(bestMatch.Answer, "fallback");
        }

        var genericAnswer =
            $"I could not find an exact answer in the knowledge base. " +
            $"Please email {knowledgeBase.Contact.Email} or use the website contact form. " +
            $"The team usually responds within {knowledgeBase.Contact.ResponseTime}.";

        return AnswerResult.Ok(genericAnswer, "fallback");
    }

    private static FaqItem? FindBestMatch(string userMessage, IEnumerable<FaqItem> faqs)
    {
        var lowerMessage = userMessage.ToLowerInvariant();
        var tokens = Tokenize(userMessage);

        double bestScore = 0;
        FaqItem? bestFaq = null;

        foreach (var faq in faqs)
        {
            var score = CalculateScore(lowerMessage, tokens, faq);

            if (score > bestScore)
            {
                bestScore = score;
                bestFaq = faq;
            }
        }

        return bestScore > 0 ? bestFaq : null;
    }

    private static List<string> Tokenize(string text)
    {
        return Regex.Matches(text.ToLowerInvariant(), @"[a-z0-9\+\-]+")
            .Select(match => match.Value)
            .Where(token => token.Length > 2 && !StopWords.Contains(token))
            .Distinct()
            .ToList();
    }

    private static double CalculateScore(string lowerMessage, List<string> tokens, FaqItem faq)
    {
        double score = 0;

        var question = faq.Question.ToLowerInvariant();
        var answer = faq.Answer.ToLowerInvariant();
        var category = faq.Category.ToLowerInvariant();
        var tags = faq.Metadata?.Tags?.Select(tag => tag.ToLowerInvariant()).ToList() ?? new List<string>();

        foreach (var token in tokens)
        {
            if (question.Contains(token))
            {
                score += 3;
            }

            if (answer.Contains(token))
            {
                score += 2;
            }

            if (category.Contains(token))
            {
                score += 2;
            }

            if (tags.Any(tag => tag.Contains(token)))
            {
                score += 4;
            }
        }

        // Extra scoring for critical support questions.
        if (faq.Metadata?.IsCritical == true &&
            (lowerMessage.Contains("down") ||
             lowerMessage.Contains("slow") ||
             lowerMessage.Contains("not working") ||
             lowerMessage.Contains("terrible")))
        {
            score += 8;
        }

        // Bonus for exact phrase overlaps.
        if (!string.IsNullOrWhiteSpace(lowerMessage))
        {
            if (question.Contains(lowerMessage))
            {
                score += 6;
            }

            if (answer.Contains(lowerMessage))
            {
                score += 4;
            }
        }

        return score;
    }
}