using CoreMod.Models.RagQuery;

namespace CoreMod.Abstraction;

/// <summary>
/// RAG 查询服务接口 - CoreMod 定义，由 KnowledgeBaseMod 实现
/// </summary>
public interface IRagQueryService
{
    /// <summary>
    /// 执行 RAG 查询
    /// </summary>
    /// <param name="query">查询请求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>查询结果</returns>
    Task<RagQueryResult> QueryAsync(RagQueryRequest query, CancellationToken cancellationToken = default);
}