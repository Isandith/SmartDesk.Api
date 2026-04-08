using System.Text.Json;
using SmartDesk.Api.Models.KnowledgeBase;

namespace SmartDesk.Api.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly string _filePath;
    private readonly Lazy<KnowledgeBaseDocument> _knowledgeBase;

    public KnowledgeBaseService(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "Knowledge-Base.json");
        _knowledgeBase = new Lazy<KnowledgeBaseDocument>(LoadKnowledgeBase);
    }

    public Task<KnowledgeBaseDocument> GetKnowledgeBaseAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_knowledgeBase.Value);
    }

    private KnowledgeBaseDocument LoadKnowledgeBase()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                $"Knowledge base file was not found at '{_filePath}'. " +
                "Create Data/Knowledge-Base.json and paste your JSON there.");
        }

        var json = File.ReadAllText(_filePath);

        var document = JsonSerializer.Deserialize<KnowledgeBaseDocument>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (document == null)
        {
            throw new InvalidOperationException("Knowledge base JSON could not be parsed.");
        }

        return document;
    }
}