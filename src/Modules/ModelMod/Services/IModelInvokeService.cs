namespace ModelMod.Services;

public interface IModelInvokeService
{
    Task<ModelResponse> ChatAsync(Guid applicationId, ModelRequest request, CancellationToken cancellationToken = default);

    Task<ModelResponse> EmbeddingAsync(Guid applicationId, ModelRequest request, CancellationToken cancellationToken = default);
}
