namespace Share.Services;

public interface IModelInvokeFacade
{
    Task<ModelInvokeResponse> ChatAsync(Guid applicationId, ModelInvokeRequest request, CancellationToken cancellationToken = default);

    Task<ModelInvokeResponse> EmbeddingAsync(Guid applicationId, ModelInvokeRequest request, CancellationToken cancellationToken = default);
}
