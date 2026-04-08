using System.Text.RegularExpressions;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Models.KnowledgeBase;
using SmartDesk.Api.Services;

namespace SmartDesk.Api.Strategies;

/// <summary>
/// Resolves answers from keyword and context matching against the knowledge base.
/// </summary>
public class KeywordFallbackStrategy
{
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "is", "a", "an", "and", "or", "to", "for", "of", "in",
        "on", "at", "my", "me", "i", "you", "we", "our", "your",
        "how", "what", "do", "does", "can", "could", "would", "should",
        "please", "help", "with", "this", "that", "it", "are", "am",
        "about", "all", "else", "more", "again", "really", "tell", "explain",
        "detail", "details", "anything", "further", "thats"
    };

    private static readonly string[] FollowUpPhrases =
    [
        "is that all",
        "anything else",
        "tell me more",
        "what about that",
        "can you explain more",
        "really",
        "thats it",
        "that's it",
        "more details",
        "what else",
        "go on",
        "and then",
        "can you tell me more"
    ];

    public KeywordFallbackStrategy(IKnowledgeBaseService knowledgeBaseService)
    {
        _knowledgeBaseService = knowledgeBaseService;
    }

    /// <summary>
    /// Tries to get an answer for the user's message, optionally using the provided chat context.
    /// </summary>
    /// <param name="userMessage">The message from the user.</param>
    /// <param name="context">The chat context, i.e., previous messages in the conversation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// An <see cref="AnswerResult"/> containing the answer, or an indication that no answer was found.
    /// </returns>
    public async Task<AnswerResult> TryGetAnswerAsync(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        CancellationToken cancellationToken = default)
    {
        var knowledgeBase = await _knowledgeBaseService.GetKnowledgeBaseAsync(cancellationToken);

        if (IsFollowUpMessage(userMessage))
        {
            var contextualAnswer = TryGetContextualAnswer(userMessage, context, knowledgeBase.Faqs);

            if (contextualAnswer != null)
            {
                return contextualAnswer;
            }
        }

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

    private static AnswerResult? TryGetContextualAnswer(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        IEnumerable<FaqItem> faqs)
    {
        var latestAssistantMessage = context
            .LastOrDefault(message => string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase));

        if (latestAssistantMessage != null)
        {
            var matchFromLatestAssistant = FindBestMatch(latestAssistantMessage.Content, faqs);

            if (matchFromLatestAssistant != null)
            {
                return AnswerResult.Ok(BuildFollowUpAnswer(matchFromLatestAssistant.Answer, userMessage), "contextual_fallback");
            }
        }

        var contextText = BuildContextReferenceText(context);
        var matchFromContext = FindBestMatch(contextText, faqs);

        if (matchFromContext != null)
        {
            return AnswerResult.Ok(BuildFollowUpAnswer(matchFromContext.Answer, userMessage), "contextual_fallback");
        }

        if (latestAssistantMessage != null && !string.IsNullOrWhiteSpace(latestAssistantMessage.Content))
        {
            return AnswerResult.Ok(BuildFollowUpAnswer(latestAssistantMessage.Content, userMessage), "contextual_fallback");
        }

        return null;
    }

    private static string BuildContextReferenceText(IReadOnlyList<ChatMessage> context)
    {
        if (context.Count == 0)
        {
            return string.Empty;
        }

        var priorMessages = context
            .Take(Math.Max(context.Count - 1, 0))
            .Where(message => !string.IsNullOrWhiteSpace(message.Content))
            .Select(message => message.Content.Trim());

        return string.Join(' ', priorMessages);
    }

    private static string BuildFollowUpAnswer(string answer, string userMessage)
    {
        var normalizedMessage = NormalizeForMatching(userMessage);
        var trimmedAnswer = answer.Trim();

        var followUpSuffix = normalizedMessage.Contains("tell me more", StringComparison.OrdinalIgnoreCase) ||
                             normalizedMessage.Contains("more details", StringComparison.OrdinalIgnoreCase) ||
                             normalizedMessage.Contains("explain more", StringComparison.OrdinalIgnoreCase) ||
                             normalizedMessage.Contains("what about", StringComparison.OrdinalIgnoreCase)
            ? "If you'd like, I can break that down further."
            : "If you'd like, I can explain it in more detail.";

        return $"{trimmedAnswer} {followUpSuffix}".Trim();
    }

    private static bool IsFollowUpMessage(string message)
    {
        var normalizedMessage = NormalizeForMatching(message);

        if (string.IsNullOrWhiteSpace(normalizedMessage))
        {
            return false;
        }

        if (FollowUpPhrases.Any(phrase => normalizedMessage.Contains(phrase, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var tokens = Tokenize(normalizedMessage);

        return tokens.Count <= 4 && tokens.Any(token =>
            token is "all" or "else" or "more" or "again" or "really" or "explain" or "details" or "detail");
    }

    private static FaqItem? FindBestMatch(string userMessage, IEnumerable<FaqItem> faqs)
    {
        var normalizedMessage = NormalizeForMatching(userMessage);
        var tokens = Tokenize(normalizedMessage);

        double bestScore = 0;
        FaqItem? bestFaq = null;

        foreach (var faq in faqs)
        {
            var score = CalculateScore(normalizedMessage, tokens, faq);

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
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length > 2 && !StopWords.Contains(token))
            .Distinct(StringComparer.OrdinalIgnoreCase)
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

    private static string NormalizeForMatching(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        var normalized = message.Trim().ToLowerInvariant().Replace("'", string.Empty);
        normalized = Regex.Replace(normalized, @"[^a-z0-9\+\-]+", " ");
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        return normalized;
    }
}