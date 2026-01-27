using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 统一模型调用入口
/// </summary>
public interface IModelClient
{
    Task<ModelResponse> ChatAsync(ModelRequest request, CancellationToken cancellationToken = default);

    Task<ModelResponse> EmbeddingAsync(ModelRequest request, CancellationToken cancellationToken = default);

    Task<ModelResponse> VisionAsync(ModelRequest request, CancellationToken cancellationToken = default);

    Task<ModelResponse> ModerationAsync(ModelRequest request, CancellationToken cancellationToken = default);
}
