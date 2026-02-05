using EntityFramework.AppDbContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Perigon.AspNetCore.Constants;

namespace EntityFramework.AppDbFactory;

/// <summary>
/// factory for create TenantDbContext
/// </summary>
/// <param name="tenantContext"></param>
/// <param name="cache"></param>
/// <param name="configuration"></param>
public class TenantDbFactory(
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    IOptions<ComponentOption> options
)
{
    public bool IsMultiTenant => options.Value.IsMultiTenant;

    public async Task<DefaultDbContext> CreateDbContextAsync()
    {
        var builder = new DbContextOptionsBuilder<DefaultDbContext>();

        using var scope = serviceScopeFactory.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        Guid tenantId = tenantContext.TenantId;

        var connectionStrings = configuration.GetConnectionString(AppConst.Default);
        if (IsMultiTenant && tenantContext.TenantType == TenantType
            .Independent
            .ToString())
        {
            connectionStrings = await tenantContext.GetDbConnectionStringAsync();
        }
        switch (options?.Value.Database)
        {
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(connectionStrings);
                break;
            case DatabaseType.SqlServer:
                builder.UseSqlServer(connectionStrings);
                break;
        }
        return new DefaultDbContext(builder.Options);
    }

    public async Task<AnalysisDbContext> CreateAnalysisDbContextAsync()
    {
        var builder = new DbContextOptionsBuilder<AnalysisDbContext>();
        using var scope = serviceScopeFactory.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        Guid tenantId = tenantContext.TenantId;
        var connectionStrings = configuration.GetConnectionString(AppConst.Analysis);
        if (IsMultiTenant && tenantContext.TenantType == TenantType
            .Independent
            .ToString())
        {
            connectionStrings = await tenantContext.GetAnalysisConnectionStringAsync();
        }
        switch (options?.Value.Database)
        {
            case DatabaseType.PostgreSql:
                builder.UseNpgsql(connectionStrings);
                break;
            case DatabaseType.SqlServer:
                builder.UseSqlServer(connectionStrings);
                break;
        }
        return new AnalysisDbContext(builder.Options);
    }
}
