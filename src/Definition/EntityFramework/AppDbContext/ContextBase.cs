using Entity.CommonMod;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EntityFramework.AppDbContext;

public abstract class ContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Tenant>().Ignore(t => t.TenantId);

        base.OnModelCreating(builder);
        OnModelExtendCreating(builder);
        OnSQLiteModelCreating(builder);
    }

    private void OnModelExtendCreating(ModelBuilder modelBuilder)
    {
        IEnumerable<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> entityTypes =
            modelBuilder.Model.GetEntityTypes();
        foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType in entityTypes)
        {
            // Skip entity types without a CLR type (shadow/relational types)
            if (entityType.ClrType == null)
            {
                continue;
            }

            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.Name).HasKey(nameof(EntityBase.Id));
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasQueryFilter(
                        ConvertFilterExpression<EntityBase>(e => !e.IsDeleted, entityType.ClrType)
                    );
            }
        }
    }

    private void OnSQLiteModelCreating(ModelBuilder modelBuilder)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType
                    .ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTimeOffset) || p.PropertyType == typeof(DateTimeOffset?));
                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(new DateTimeOffsetToStringConverter());
                }
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression<TInterface>(
        Expression<Func<TInterface, bool>> filterExpression,
        Type entityType
    )
    {
        ParameterExpression newParam = Expression.Parameter(entityType);
        Expression newBody = ReplacingExpressionVisitor.Replace(
            filterExpression.Parameters.Single(),
            newParam,
            filterExpression.Body
        );

        return Expression.Lambda(newBody, newParam);
    }
}
