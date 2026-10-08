using Perigon.AspNetCore.Constants;

namespace EntityFramework.AppDbContext;

public static class DefaultDbContextSeeding
{
    public const string DefaultTenantDomain = "default.com";

    /// <param name="analysisConnectionString">
    /// Optional analysis database connection string. The default database connection is used when omitted.
    /// </param>
    public static DbContextOptionsBuilder UseDefaultDbContextSeeding(
        this DbContextOptionsBuilder optionsBuilder,
        string? analysisConnectionString = null
    )
    {
        return optionsBuilder
            .UseSeeding((context, _) => SeedDefaultTenant(context, analysisConnectionString))
            .UseAsyncSeeding((context, _, cancellationToken) =>
                SeedDefaultTenantAsync(context, analysisConnectionString, cancellationToken)
            );
    }

    private static void SeedDefaultTenant(DbContext context, string? analysisConnectionString)
    {
        var defaultConnectionString = GetRequiredConnectionString(context);
        var defaultAnalysisConnectionString = analysisConnectionString ?? defaultConnectionString;
        var tenants = context.Set<Tenant>();
        var defaultTenant = tenants
            .IgnoreQueryFilters([ContextBase.SoftDeletionFilterName])
            .SingleOrDefault(t => t.Domain == DefaultTenantDomain);
        if (defaultTenant is not null)
        {
            var hasChanges = false;
            if (defaultTenant.IsDeleted)
            {
                defaultTenant.IsDeleted = false;
                hasChanges = true;
            }
            if (string.IsNullOrWhiteSpace(defaultTenant.DbConnectionString))
            {
                defaultTenant.DbConnectionString = defaultConnectionString;
                hasChanges = true;
            }
            if (string.IsNullOrWhiteSpace(defaultTenant.AnalysisConnectionString))
            {
                defaultTenant.AnalysisConnectionString = defaultAnalysisConnectionString;
                hasChanges = true;
            }
            if (hasChanges)
            {
                context.SaveChanges();
            }
            return;
        }

        // Tenant is the global tenant catalog root. ContextBase intentionally ignores
        // Tenant.TenantId, so this seed must not assign a tenant id to the entity.
        tenants.Add(new Tenant
        {
            Domain = DefaultTenantDomain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
            DbConnectionString = defaultConnectionString,
            AnalysisConnectionString = defaultAnalysisConnectionString,
        });

        context.SaveChanges();
    }

    private static async Task SeedDefaultTenantAsync(
        DbContext context,
        string? analysisConnectionString,
        CancellationToken cancellationToken
    )
    {
        var defaultConnectionString = GetRequiredConnectionString(context);
        var defaultAnalysisConnectionString = analysisConnectionString ?? defaultConnectionString;
        var tenants = context.Set<Tenant>();
        var defaultTenant = await tenants
            .IgnoreQueryFilters([ContextBase.SoftDeletionFilterName])
            .SingleOrDefaultAsync(t => t.Domain == DefaultTenantDomain, cancellationToken);
        if (defaultTenant is not null)
        {
            var hasChanges = false;
            if (defaultTenant.IsDeleted)
            {
                defaultTenant.IsDeleted = false;
                hasChanges = true;
            }
            if (string.IsNullOrWhiteSpace(defaultTenant.DbConnectionString))
            {
                defaultTenant.DbConnectionString = defaultConnectionString;
                hasChanges = true;
            }
            if (string.IsNullOrWhiteSpace(defaultTenant.AnalysisConnectionString))
            {
                defaultTenant.AnalysisConnectionString = defaultAnalysisConnectionString;
                hasChanges = true;
            }
            if (hasChanges)
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            return;
        }

        // Keep the async path equivalent to the synchronous path used by EF tooling.
        tenants.Add(new Tenant
        {
            Domain = DefaultTenantDomain,
            Name = AppConst.Default,
            Description = "This is default tenant, created by system.",
            DbConnectionString = defaultConnectionString,
            AnalysisConnectionString = defaultAnalysisConnectionString,
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string GetRequiredConnectionString(DbContext context)
    {
        var connectionString = context.Database.GetConnectionString();
        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException(
                "The default tenant cannot be initialized because the database connection string is unavailable."
            );
    }

}
