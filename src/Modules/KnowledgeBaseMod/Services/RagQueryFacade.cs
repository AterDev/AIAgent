using KnowledgeBaseMod.Models.RagQueryDtos;

namespace KnowledgeBaseMod.Services;

/// <summary>
/// Share 接口适配器（RAG 查询）
/// </summary>
public class RagQueryFacade(IRagQueryService service) : Share.Services.IRagQueryFacade
{
    public async Task<Share.Services.RagQueryResult> QueryAsync(Share.Services.RagQueryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await service.QueryAsync(new RagQueryRequest
        {
            Query = request.Query,
            CollectionId = request.CollectionId,
            TopK = request.TopK,
        }, cancellationToken);

        return new Share.Services.RagQueryResult
        {
            Items = result.Items.Select(i => new Share.Services.RagQueryItem
            {
                DocumentId = i.DocumentId,
                Content = i.Content,
                Score = i.Score,
            }).ToList(),
        };
    }
}
