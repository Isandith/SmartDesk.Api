using SmartDesk.Api.Models.KnowledgeBase;

namespace SmartDesk.Api.Services;

public interface IKnowledgeBaseService
{
    Task<KnowledgeBaseDocument> GetKnowledgeBaseAsync(CancellationToken cancellationToken = default);
}