using KnowledgeBaseMod.Models.RagQueryDtos;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// 知识库检索服务
/// </summary>
public interface IRagQueryService
{
    Task<RagQueryResult> QueryAsync(RagQueryRequest request, CancellationToken cancellationToken = default);
}
