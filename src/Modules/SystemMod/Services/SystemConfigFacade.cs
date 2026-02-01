using Share.Services;

namespace SystemMod.Services;

/// <summary>
/// 系统配置读取与模板渲染
/// </summary>
public class SystemConfigFacade(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    ILogger<SystemConfigFacade> logger
)
{
    public async Task<string?> GetValueAsync(string groupName, string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var value = await dbContext.SystemConfigs
            .AsNoTracking()
            .Where(q => q.TenantId == userContext.TenantId
                && q.GroupName == groupName
                && q.Key == key
                && q.Valid)
            .Select(q => q.Value)
            .FirstOrDefaultAsync(cancellationToken);

        logger.LogDebug("Read SystemConfig {Group}/{Key}", groupName, key);
        return value;
    }

    public string RenderTemplate(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(template) || variables.Count == 0)
        {
            return template;
        }

        var result = template;
        foreach (var pair in variables)
        {
            result = result.Replace("{{" + pair.Key + "}}", pair.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
