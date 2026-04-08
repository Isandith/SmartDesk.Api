using System.Text;
using System.Text.Json;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Models.KnowledgeBase;

namespace SmartDesk.Api.Adapters;

public class GeminiServiceAdapter : IAiServiceAdapter
{
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiServiceAdapter> _logger;
    private readonly HttpClient _httpClient;

    public GeminiServiceAdapter(ILogger<GeminiServiceAdapter> logger)
    {
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        _model = Environment.GetEnvironmentVariable("LLM_MODEL") ?? "gemini-2.0-flash";
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<AiServiceResult> GetAnswerAsync(
        string userMessage,
        IReadOnlyList<ChatMessage> context,
        KnowledgeBaseDocument knowledgeBase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("Gemini API key is not configured. Falling back to keyword matching.");
                return AiServiceResult.Fail(AiServiceFailureReason.MissingApiKey);
            }

            // Build context from knowledge base
            var knowledgeContext = BuildKnowledgeContext(knowledgeBase);

            // Build conversation history
            var conversationHistory = BuildConversationHistory(context);

            // Create the system prompt with knowledge base context
            var systemPrompt = $"""
                You are a helpful FAQ assistant for {knowledgeBase.CompanyName}.
                Use the following knowledge base to answer user questions accurately and concisely.
                
                KNOWLEDGE BASE:
                {knowledgeContext}
                """;

            // Build request body for Gemini API
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt },
                            new { text = conversationHistory },
                            new { text = userMessage }
                        }
                    }
                },
                generationConfig = new
                {
                    maxOutputTokens = 1024,
                    temperature = 0.7f
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Call Gemini API
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API returned error: {StatusCode} - {Error}", response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                    errorContent.Contains("API key not valid", StringComparison.OrdinalIgnoreCase) ||
                    errorContent.Contains("invalid api key", StringComparison.OrdinalIgnoreCase) ||
                    errorContent.Contains("permission denied", StringComparison.OrdinalIgnoreCase) ||
                    errorContent.Contains("API_KEY_INVALID", StringComparison.OrdinalIgnoreCase))
                {
                    return AiServiceResult.Fail(AiServiceFailureReason.InvalidApiKey);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    errorContent.Contains("quota", StringComparison.OrdinalIgnoreCase) ||
                    errorContent.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
                    errorContent.Contains("TooManyRequests", StringComparison.OrdinalIgnoreCase) ||
                    errorContent.Contains("RESOURCE_EXHAUSTED", StringComparison.OrdinalIgnoreCase))
                {
                    return AiServiceResult.Fail(AiServiceFailureReason.QuotaExceeded);
                }

                return AiServiceResult.Fail(AiServiceFailureReason.ApiError);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonResponse = JsonDocument.Parse(responseContent);
            
            // Extract the text from the Gemini response
            if (jsonResponse.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var candidateContent) &&
                    candidateContent.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    if (parts[0].TryGetProperty("text", out var text))
                    {
                        var answer = text.GetString();
                        if (!string.IsNullOrWhiteSpace(answer))
                        {
                            _logger.LogInformation("Gemini API returned answer for message: {Message}", userMessage);
                            return AiServiceResult.Ok(answer);
                        }
                    }
                }
            }

            _logger.LogWarning("Gemini API returned empty response for message: {Message}", userMessage);
            return AiServiceResult.Fail(AiServiceFailureReason.EmptyResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API for message: {Message}", userMessage);
            return AiServiceResult.Fail(AiServiceFailureReason.Exception);
        }
    }

    private static string BuildKnowledgeContext(KnowledgeBaseDocument knowledgeBase)
    {
        if (knowledgeBase?.Faqs == null || !knowledgeBase.Faqs.Any())
        {
            return "No FAQs available.";
        }

        var faqTexts = knowledgeBase.Faqs
            .Select(faq => $"Q: {faq.Question}\nA: {faq.Answer}\nCategory: {faq.Category}")
            .Take(20); // Limit to first 20 FAQs to avoid token overflow

        return string.Join("\n\n", faqTexts);
    }

    private static string BuildConversationHistory(IReadOnlyList<ChatMessage> context)
    {
        if (!context.Any())
        {
            return "No previous messages.";
        }

        var messages = context
            .Select(msg => $"{msg.Role.ToUpper()}: {msg.Content}")
            .ToList();

        return string.Join("\n", messages);
    }
}
