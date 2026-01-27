using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 模型能力解析
/// </summary>
public interface IModelCapabilityResolver
{
    Task<ModelCapability> ResolveAsync(string model, CancellationToken cancellationToken = default);
}
