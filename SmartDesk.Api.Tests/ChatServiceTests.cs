using SmartDesk.Api.Adapters;
using SmartDesk.Api.Models.Chat;
using SmartDesk.Api.Models.KnowledgeBase;
using SmartDesk.Api.Services;
using SmartDesk.Api.Strategies;
using SmartDesk.Api.Validators;
using Xunit;

namespace SmartDesk.Api.Tests;

public class ChatServiceTests
{
    [Fact]
    public async Task AskAsync_WhenAiFails_UsesKeywordFallbackAndExposesStatusMessage()
    {
        var service = CreateChatService(
            AiServiceResult.Fail(AiServiceFailureReason.MissingApiKey),
            CreateKnowledgeBase());

        var response = await service.AskAsync(new ChatRequest
        {
            Message = "What core services does Ekara Digital offer?"
        });

        Assert.Equal("fallback", response.ResponseSource);
        Assert.True(response.ManualMode);
        Assert.False(string.IsNullOrWhiteSpace(response.SystemStatusMessage));
        Assert.Contains("Software Development", response.Answer);
        Assert.DoesNotContain("!$!@$!$!", response.Answer);
        Assert.DoesNotContain("[WARNING]", response.Answer);
        Assert.DoesNotContain("[CONTENT]", response.Answer);
    }

    [Fact]
    public async Task AskAsync_WhenUserIsFrustrated_PrefixesPrioritySupportMessage()
    {
        var service = CreateChatService(
            AiServiceResult.Fail(AiServiceFailureReason.MissingApiKey),
            CreateKnowledgeBase());

        var response = await service.AskAsync(new ChatRequest
        {
            Message = "This system is terrible and not working!"
        });

        Assert.True(response.PriorityEscalation);
        Assert.StartsWith("⚠️ Priority Support:", response.Answer);
        Assert.Equal(-1, response.SentimentScore);
        Assert.Contains("24-hour support", response.Answer, StringComparison.OrdinalIgnoreCase);
    }

    private static ChatService CreateChatService(AiServiceResult aiResult, KnowledgeBaseDocument knowledgeBase)
    {
        var knowledgeBaseService = new StubKnowledgeBaseService(knowledgeBase);
        var aiAdapter = new StubAiServiceAdapter(aiResult);

        return new ChatService(
            new InMemorySessionService(),
            new RuleBasedSentimentService(),
            new AiAnswerStrategy(aiAdapter, knowledgeBaseService),
            new KeywordFallbackStrategy(knowledgeBaseService),
            new ChatRequestValidator(),
            new ChatResponseValidator());
    }

    private static KnowledgeBaseDocument CreateKnowledgeBase()
    {
        return new KnowledgeBaseDocument
        {
            CompanyName = "Ekara Digital Partners",
            Contact = new ContactInfo
            {
                Email = "info@ekara.nz",
                ResponseTime = "2-3 business days"
            },
            Faqs =
            [
                new FaqItem
                {
                    Id = 1,
                    Category = "services",
                    Question = "What core services does Ekara Digital offer?",
                    Answer = "We specialize in custom Software Development, AI & Machine Learning solutions, Cloud Infrastructure design, ERP/CRM support, and Digital Marketing strategies.",
                    Metadata = new FaqMetadata
                    {
                        Tags = ["software", "AI", "cloud", "marketing"]
                    }
                },
                new FaqItem
                {
                    Id = 9,
                    Category = "support",
                    Question = "My system is down or extremely slow.",
                    Answer = "For critical system failures, Ekara provides 24-hour support. Please ensure your internet connection is stable; if the issue persists, our engineers will intervene.",
                    Metadata = new FaqMetadata
                    {
                        Tags = ["emergency", "slow", "down"],
                        IsCritical = true
                    }
                }
            ]
        };
    }

    private sealed class StubAiServiceAdapter : IAiServiceAdapter
    {
        private readonly AiServiceResult _result;

        public StubAiServiceAdapter(AiServiceResult result)
        {
            _result = result;
        }

        public Task<AiServiceResult> GetAnswerAsync(
            string userMessage,
            IReadOnlyList<ChatMessage> context,
            KnowledgeBaseDocument knowledgeBase,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class StubKnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly KnowledgeBaseDocument _knowledgeBase;

        public StubKnowledgeBaseService(KnowledgeBaseDocument knowledgeBase)
        {
            _knowledgeBase = knowledgeBase;
        }

        public Task<KnowledgeBaseDocument> GetKnowledgeBaseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_knowledgeBase);
        }
    }
}