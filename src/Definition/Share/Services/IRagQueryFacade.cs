namespace Share.Services;

public interface IRagQueryFacade
{
    Task<RagQueryResult> QueryAsync(RagQueryRequest request, CancellationToken cancellationToken = default);
}
