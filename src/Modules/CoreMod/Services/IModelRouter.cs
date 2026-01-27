using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 模型路由
/// </summary>
public interface IModelRouter
{
    Task<ModelRoute> ResolveAsync(ModelRequest request, CancellationToken cancellationToken = default);
}
