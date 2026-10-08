using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFramework.AppDbContext;

public abstract class ContextBase : DbContext
{
    public const string SoftDeletionFilterName = "SoftDeletionFilter";
    public const string TenantFilterName = "TenantFilter";

    protected ContextBase(DbContextOptions options)
        : base(options)
    {
    }

    /// <summary>
    /// The tenant associated with this context. A null value means this is a tenant
    /// catalog/design-time context and tenant-scoped entities must not be queried or written.
    /// </summary>
    public Guid? CurrentTenantId { get; private set; }

    /// <summary>
    /// Associates this context with a tenant before it is used for tenant-scoped work.
    /// AppDbFactory calls this immediately after creating a non-pooled context.
    /// </summary>
    public void SetTenantId(Guid? tenantId)
    {
        Guid? normalizedTenantId = tenantId is { } value && value != Guid.Empty ? value : null;
        if (CurrentTenantId is { } currentTenantId && currentTenantId != normalizedTenantId)
        {
            throw new InvalidOperationException("The tenant of a DbContext cannot be changed.");
        }

        CurrentTenantId = normalizedTenantId;
    }

    public DbSet<Tenant> Tenants { get; set; }

    public override int SaveChanges()
    {
        return SaveChanges(acceptAllChangesOnSuccess: true);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyTenantOwnership();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        ApplyTenantOwnership();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Conventions.Add(serviceProvider =>
            new TenantIndexConvention(
                serviceProvider.GetRequiredService<IDatabaseProvider>().Name
            )
        );
        configurationBuilder.Conventions.Add(_ => new TenantQueryFilterConvention(this));
    }

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
                modelBuilder.Entity(entityType.ClrType).HasKey(nameof(EntityBase.Id));
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

    internal static LambdaExpression ConvertFilterExpression<TInterface>(
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

    internal LambdaExpression CreateTenantFilter(Type entityType)
    {
        return ConvertFilterExpression<ITenantEntityBase>(
            entity => entity.TenantId == CurrentTenantId,
            entityType
        );
    }

    private void ApplyTenantOwnership()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntityBase>())
        {
            // Tenant is the root tenant catalog and its inherited TenantId property is
            // intentionally ignored by the model.
            if (entry.Entity is Tenant)
            {
                continue;
            }

            if (CurrentTenantId is not Guid tenantId)
            {
                throw new InvalidOperationException(
                    $"A TenantId is required to persist {entry.Metadata.ClrType.Name}."
                );
            }

            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
                continue;
            }

            if (entry.Entity.TenantId != tenantId)
            {
                throw new InvalidOperationException(
                    $"The entity {entry.Metadata.ClrType.Name} does not belong to the current tenant."
                );
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(ITenantEntityBase.TenantId)).IsModified = false;
            }
        }
    }
}
