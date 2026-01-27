using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 默认模型能力解析（占位）
/// </summary>
public class DefaultModelCapabilityResolver : IModelCapabilityResolver
{
    public Task<ModelCapability> ResolveAsync(string model, CancellationToken cancellationToken = default)
    {
        var capability = new ModelCapability
        {
            SupportsChat = true,
            SupportsEmbedding = true,
            SupportsTools = true,
            SupportsVision = true,
            SupportsResponsesApi = true,
        };
        return Task.FromResult(capability);
    }
}
