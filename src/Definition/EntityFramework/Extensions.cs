namespace EntityFramework;

public static class Extensions
{
    public static async Task<int> PartialUpdateAsync<TEntity, TUpdateDto>(
        this DbContext db,
        Guid id,
        TUpdateDto dto,
        bool updateUpdatedTime = true,
        CancellationToken cancellationToken = default,
        Guid? tenantId = null
    )
        where TEntity : class, IEntityBase
        where TUpdateDto : class
    {
        var set = db.Set<TEntity>();

        var eParam = Expression.Parameter(typeof(TEntity), "e");
        var keyProp = Expression.Property(eParam, nameof(EntityBase.Id));
        var idConst = Expression.Constant(id, typeof(Guid));
        var equal = Expression.Equal(keyProp, idConst);
        Expression whereExpression = equal;
        if (tenantId.HasValue && typeof(ITenantEntityBase).IsAssignableFrom(typeof(TEntity)))
        {
            var tenantProp = Expression.Property(eParam, nameof(ITenantEntityBase.TenantId));
            var tenantConst = Expression.Constant(tenantId.Value, typeof(Guid));
            whereExpression = Expression.AndAlso(
                whereExpression,
                Expression.Equal(tenantProp, tenantConst)
            );
        }

        var whereLambda = Expression.Lambda<Func<TEntity, bool>>(whereExpression, eParam);

        return await set.Where(whereLambda).ExecuteUpdateAsync(updater =>
        {
            var type = updater.GetType();
            var setPropertyMethod = type.GetMethods()
                .First(m => m.Name == "SetProperty"
                            && m.IsGenericMethod
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[1].ParameterType == m.GetGenericArguments()[0]);

            foreach (var dtoProp in typeof(TUpdateDto).GetProperties())
            {
                // Ownership and identity are never part of a partial business update.
                if (dtoProp.Name is nameof(IEntityBase.Id)
                    or nameof(ITenantEntityBase.TenantId)
                    or nameof(IEntityBase.CreatedTime))
                {
                    continue;
                }

                var value = dtoProp.GetValue(dto);
                if (value is null)
                {
                    continue;
                }

                var entityProp = typeof(TEntity).GetProperty(dtoProp.Name);
                if (entityProp is null)
                {
                    continue;
                }

                // Construct e => e.Prop
                var propExp = Expression.Property(eParam, entityProp);
                var propLambda = Expression.Lambda(propExp, eParam);

                // Invoke SetProperty<TProp>(lambda, value)
                var genericSetProperty = setPropertyMethod.MakeGenericMethod(entityProp.PropertyType);
                genericSetProperty.Invoke(updater, [propLambda, value]);
            }

            if (updateUpdatedTime)
            {
                var entityProp = typeof(TEntity).GetProperty(nameof(EntityBase.UpdatedTime));
                if (entityProp != null)
                {
                    var propExp = Expression.Property(eParam, entityProp);
                    var propLambda = Expression.Lambda(propExp, eParam);

                    var genericSetProperty = setPropertyMethod.MakeGenericMethod(entityProp.PropertyType);
                    genericSetProperty.Invoke(updater, [propLambda, DateTimeOffset.UtcNow]);
                }
            }
        }, cancellationToken);
    }
}
