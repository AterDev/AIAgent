using KnowledgeBaseMod.Models.RagQueryDtos;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 空实现（占位）
/// </summary>
public class NullRagQueryService : IRagQueryService
{
    public Task<RagQueryResult> QueryAsync(RagQueryRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new RagQueryResult());
    }
}
