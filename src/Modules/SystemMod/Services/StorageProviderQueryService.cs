namespace SystemMod.Services;
using Share.Abstraction;
using Share.Models;

/// <summary>
/// 存储提供商查询服务 - 为核心模块提供跨模块数据访问
/// </summary>
public class StorageProviderQueryService(
    TenantDbFactory dbContextFactory
) : IStorageProviderQuery
{
    public async Task<StorageProviderInfo?> GetProviderAsync(Guid storageProviderId, CancellationToken cancellationToken = default)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var provider = await dbContext.Set<StorageProvider>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == storageProviderId, cancellationToken);

        if (provider == null)
        {
            return null;
        }

        return new StorageProviderInfo
        {
            Id = provider.Id,
            Name = provider.Name,
            IsCloud = provider.IsCloud,
            IsActive = provider.IsActive
        };
    }

    public async Task<StorageProviderInfo?> GetActiveProviderAsync(CancellationToken cancellationToken = default)
    {
        using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var provider = await dbContext.Set<StorageProvider>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsActive, cancellationToken);

        if (provider == null)
        {
            return null;
        }

        return new StorageProviderInfo
        {
            Id = provider.Id,
            Name = provider.Name,
            IsCloud = provider.IsCloud,
            IsActive = provider.IsActive
        };
    }
}
