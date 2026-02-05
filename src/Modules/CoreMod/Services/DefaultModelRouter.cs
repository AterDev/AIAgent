using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 默认模型路由（占位）
/// </summary>
public class DefaultModelRouter
{
    public Task<ModelRoute> ResolveAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        var route = new ModelRoute
        {
            Provider = request.Provider ?? string.Empty,
        };
        return Task.FromResult(route);
    }
}
