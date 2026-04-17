namespace CoreMod.Services.ModelRouting;

/// <summary>
/// 基于数据库的模型路由
/// </summary>
public class DbModelRouter(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<DbModelRouter> logger
)
{
    public async Task<ModelRoute> ResolveAsync(ModelRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var isGuid = Guid.TryParse(request.Model, out var modelId);

        var modelInfo = await dbContext.AIModelInfos
            .AsNoTracking()
            .Include(m => m.Provider)
            .Where(m => m.TenantId == userContext.TenantId
                && m.IsEnabled
                && (isGuid ? m.Id == modelId : m.Name == request.Model)
                && m.Provider != null
                && m.Provider.TenantId == userContext.TenantId)
            .Where(m => string.IsNullOrWhiteSpace(request.Provider) || m.Provider!.Name == request.Provider)
            .FirstOrDefaultAsync(cancellationToken);

        if (modelInfo?.Provider is null)
        {
            logger.LogWarning("Model route not found for model {Model} (Parsed as {Type})",
                request.Model, isGuid ? "ID" : "Name");
            return new ModelRoute
            {
                Provider = request.Provider ?? string.Empty,
            };
        }

        return new ModelRoute
        {
            Provider = modelInfo.Provider.Name,
            BaseUrl = modelInfo.Provider.BaseUrl,
            ApiKey = modelInfo.Provider.ApiKey,
            ProviderType = modelInfo.Provider.ProviderType,
        };
    }
}
