using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 空实现，用于占位
/// </summary>
public class NullModelClient : IModelClient
{
    public Task<ModelResponse> ChatAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failed("ChatAsync not configured"));
    }

    public Task<ModelResponse> EmbeddingAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failed("EmbeddingAsync not configured"));
    }

    public Task<ModelResponse> VisionAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failed("VisionAsync not configured"));
    }

    public Task<ModelResponse> ModerationAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Failed("ModerationAsync not configured"));
    }

    private static ModelResponse Failed(string message)
    {
        return new ModelResponse
        {
            Success = false,
            ErrorMessage = message,
        };
    }
}
