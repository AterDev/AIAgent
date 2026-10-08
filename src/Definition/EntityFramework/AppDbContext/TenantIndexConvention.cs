using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Perigon.AspNetCore.Abstraction;

namespace EntityFramework.AppDbContext;

internal sealed class TenantIndexConvention : IModelFinalizingConvention
{
    private readonly string _providerName;

    public TenantIndexConvention(string providerName)
    {
        _providerName = providerName;
    }

    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context
    )
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes().ToList())
        {
            if (entityType.ClrType is null || !typeof(ITenantEntityBase).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var tenantProperty = entityType.FindProperty(nameof(ITenantEntityBase.TenantId));
            if (tenantProperty is null)
            {
                continue;
            }

            foreach (var index in entityType.GetDeclaredIndexes().ToList())
            {
                if (index.Properties.Any(property => property.Name == tenantProperty.Name))
                {
                    continue;
                }

                var properties = new[] { tenantProperty }.Concat(index.Properties).ToArray();
                var isUnique = index.IsUnique;
                var databaseName = index.GetDatabaseName();
                var filter = index.GetFilter();
                var descending = index.IsDescending is null
                    ? null
                    : new[] { false }.Concat(index.IsDescending).ToArray();

                entityType.RemoveIndex(index);

                var indexBuilder = entityType.Builder.HasIndex(properties);
                if (indexBuilder is null)
                {
                    continue;
                }

                foreach (var annotation in index.GetAnnotations())
                {
                    indexBuilder.HasAnnotation(annotation.Name, annotation.Value);
                }

                indexBuilder.IsUnique(isUnique);
                indexBuilder.IsDescending(descending);
                if (databaseName is not null)
                {
                    indexBuilder.HasDatabaseName(databaseName);
                }

                if (isUnique)
                {
                    filter ??= GetUniqueIndexFilter();
                }

                if (filter is not null)
                {
                    // Keep the filter at data-annotation precedence because SQL Server's
                    // provider convention can otherwise clear it later in model building.
                    indexBuilder.HasFilter(filter, fromDataAnnotation: true);
                }
            }
        }
    }

    private string? GetUniqueIndexFilter()
    {
        return _providerName switch
        {
            "Npgsql.EntityFrameworkCore.PostgreSQL" => "\"IsDeleted\" = false",
            "Microsoft.EntityFrameworkCore.SqlServer" => "[IsDeleted] = 0",
            "Microsoft.EntityFrameworkCore.Sqlite" => "\"IsDeleted\" = 0",
            _ => null,
        };
    }
}
