using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EntityFramework.AppDbContext;

/// <summary>
/// Applies the soft-deletion and tenant filters after all entity registrations are complete.
/// </summary>
internal sealed class TenantQueryFilterConvention(ContextBase dbContext) : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context
    )
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes().ToList())
        {
            if (entityType.ClrType is null)
            {
                continue;
            }

            var builder = entityType.Builder;
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                builder.HasQueryFilter(
                    ContextBase.SoftDeletionFilterName,
                    ContextBase.ConvertFilterExpression<EntityBase>(
                        entity => !entity.IsDeleted,
                        entityType.ClrType
                    ),
                    fromDataAnnotation: false
                );
            }

            // Tenant is the root catalog and must remain visible to the middleware,
            // claims transformation, and migration seeding code.
            if (
                entityType.ClrType == typeof(Tenant)
                || !typeof(ITenantEntityBase).IsAssignableFrom(entityType.ClrType)
            )
            {
                continue;
            }

            builder.HasQueryFilter(
                ContextBase.TenantFilterName,
                dbContext.CreateTenantFilter(entityType.ClrType),
                fromDataAnnotation: false
            );
        }
    }
}
