using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace MigrationService.DesignTime;

internal class MigrationsModelDifferProxy : DispatchProxy
{
    private object _inner = null!;

    public static T Create<T>(object inner) where T : class
    {
        var proxy = DispatchProxy.Create<T, MigrationsModelDifferProxy>() as MigrationsModelDifferProxy;
        if (proxy == null)
        {
            throw new InvalidOperationException("Unable to create proxy");
        }

        proxy._inner = inner ?? throw new ArgumentNullException(nameof(inner));
        return proxy as T ?? throw new InvalidOperationException("Could not cast proxy to target interface");
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        ArgumentNullException.ThrowIfNull(targetMethod);
        try
        {
            var result = targetMethod.Invoke(_inner, args);

            if (result == null)
            {
                return null;
            }

            if (result is IEnumerable<MigrationOperation> enumOps && !(result is string))
            {
                var list = enumOps.ToList();
                var filtered = FilterOperations(list);

                // match return type
                var returnType = targetMethod.ReturnType;
                if (returnType.IsAssignableFrom(filtered.GetType()))
                {
                    return filtered;
                }

                if (returnType.IsArray)
                {
                    return filtered.ToArray();
                }

                return filtered;
            }

            return result;
        }
        catch (TargetInvocationException tie)
        {
            throw tie.InnerException ?? tie;
        }
    }

    private IReadOnlyList<MigrationOperation> FilterOperations(IReadOnlyList<MigrationOperation> ops)
    {
        try
        {
            var list = ops.ToList();

            // 1. 识别所有包含 TenantId 的表
            var tablesWithTenantId = new HashSet<string>();
            foreach (var op in list.OfType<CreateTableOperation>())
            {
                if (op.Columns.Any(c => string.Equals(c.Name, WebConst.TenantId, StringComparison.Ordinal)))
                {
                    tablesWithTenantId.Add(op.Name);
                }
            }

            var tenantSignatures = new HashSet<string>();
            foreach (var ci in list.OfType<CreateIndexOperation>())
            {
                if (ci.Columns != null && ci.Columns.Length > 0 && string.Equals(ci.Columns[0], WebConst.TenantId, StringComparison.Ordinal))
                {
                    var trailing = string.Join("|", ci.Columns.Skip(1));
                    var key = ci.Table + "::" + trailing;
                    tenantSignatures.Add(key);
                }
            }

            var dbEnv = Environment.GetEnvironmentVariable("Components__Database") ?? "PostgreSQL";
            dbEnv = dbEnv?.ToLowerInvariant() ?? string.Empty;
            string uniqueFilter = dbEnv.Equals("postgresql") ? "\"IsDeleted\" = false"
                : dbEnv.Equals("sqlserver") ? "[IsDeleted] = 0"
                : "`IsDeleted` = 0";

            var toRemove = new List<CreateIndexOperation>();
            var toAdd = new List<(int Index, CreateIndexOperation NewOp)>();

            for (int idx = 0; idx < list.Count; idx++)
            {
                var op = list[idx];
                if (op is CreateIndexOperation ci)
                {
                    // 2. 只处理那些属于多租户表的索引
                    if (!tablesWithTenantId.Contains(ci.Table))
                    {
                        continue;
                    }

                    if (ci.Columns == null || ci.Columns.Length == 0)
                    {
                        continue;
                    }

                    if (string.Equals(ci.Columns[0], WebConst.TenantId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var trailing = string.Join("|", ci.Columns);
                    var signature = ci.Table + "::" + trailing;

                    if (tenantSignatures.Contains(signature))
                    {
                        toRemove.Add(ci);
                        continue;
                    }

                    var newCols = new string[ci.Columns.Length + 1];
                    newCols[0] = WebConst.TenantId;
                    Array.Copy(ci.Columns, 0, newCols, 1, ci.Columns.Length);

                    var newCi = new CreateIndexOperation
                    {
                        Name = ci.Name,
                        Table = ci.Table,
                        Schema = ci.Schema,
                        Columns = newCols,
                        IsUnique = ci.IsUnique,
                        Filter = ci.IsUnique ? uniqueFilter : ci.Filter,
                    };

                    try
                    {
                        foreach (var ann in ci.GetAnnotations())
                        {
                            newCi.SetAnnotation(ann.Name, ann.Value);
                        }
                    }
                    catch { }

                    toAdd.Add((idx, newCi));
                    toRemove.Add(ci);
                }
            }

            foreach (var r in toRemove)
            {
                list.Remove(r);
            }

            foreach (var item in toAdd.OrderBy(i => i.Index))
            {
                var insertPos = Math.Min(item.Index, list.Count);
                list.Insert(insertPos, item.NewOp);
            }

            return list;
        }
        catch (Exception ex)
        {
            Console.WriteLine("FilterOperations exception: " + ex);
            return ops;
        }
    }
}
