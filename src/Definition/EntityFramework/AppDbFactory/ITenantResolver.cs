using Entity;

namespace EntityFramework.AppDbFactory;

/// <summary>
/// Resolves tenant metadata for components that must create a context synchronously.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Gets tenant metadata from cache or the tenant catalog.
    /// </summary>
    Tenant? GetById(Guid tenantId);
}
