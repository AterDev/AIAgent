using CoreMod.Models;

namespace CoreMod.Services;

/// <summary>
/// 基于数据库的模型路由
/// </summary>
public class DbModelRouter(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<DbModelRouter> logger
) : IModelRouter
{
    public async Task<ModelRoute> ResolveAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var query = from modelInfo in dbContext.AIModelInfos.AsNoTracking()
                    join provider in dbContext.ModelProviders.AsNoTracking()
                        on modelInfo.ProviderId equals provider.Id
                    where modelInfo.TenantId == userContext.TenantId
                        && provider.TenantId == userContext.TenantId
                        && modelInfo.IsEnabled
                        && provider.IsEnabled
                        && modelInfo.Name == request.Model
                    select new { modelInfo, provider };

        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            query = query.Where(q => q.provider.Name == request.Provider);
        }

        var result = await query.FirstOrDefaultAsync(cancellationToken);
        if (result is null)
        {
            logger.LogWarning("Model route not found for model {Model}", request.Model);
            return new ModelRoute
            {
                Provider = request.Provider ?? string.Empty,
            };
        }

        return new ModelRoute
        {
            Provider = result.provider.Name,
            BaseUrl = result.provider.BaseUrl,
            ApiKey = result.provider.ApiKey,
        };
    }
}
