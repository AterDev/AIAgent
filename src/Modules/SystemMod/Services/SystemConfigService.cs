using CoreMod.Abstraction;

namespace SystemMod.Services;

/// <summary>
/// 系统配置服务实现 - 由 ISystemConfigService 接口定义
/// </summary>
public class SystemConfigService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<SystemConfigService> logger
) : ISystemConfigService
{
    public async Task<string?> GetValueAsync(string category, string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var value = await dbContext.SystemConfigs
            .AsNoTracking()
            .Where(q => q.TenantId == userContext.TenantId
                && q.GroupName == category
                && q.Key == key
                && q.Valid)
            .Select(q => q.Value)
            .FirstOrDefaultAsync(cancellationToken);

        logger.LogDebug("Read SystemConfig {Category}/{Key}", category, key);
        return value;
    }

    public string RenderTemplate(string template, Dictionary<string, string> data)
    {
        if (string.IsNullOrWhiteSpace(template) || data.Count == 0)
        {
            return template;
        }

        var result = template;
        foreach (var pair in data)
        {
            result = result.Replace("{{" + pair.Key + "}}", pair.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}

