using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Perigon.AspNetCore.SourceGeneration;

[Generator(LanguageNames.CSharp)]
public class ManagerSourceGen : IIncrementalGenerator
{
    public const string BaseManagerName = "ManagerBase";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Step 1: 从当前项目中查找 Manager 类（转换为纯数据）
        var localManagersProvider = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => HasBaseList(node),
                transform: static (ctx, cancellationToken) =>
                {
                    var classDecl = (ClassDeclarationSyntax)ctx.Node;

                    if (
                        ctx.SemanticModel.GetDeclaredSymbol(classDecl, cancellationToken)
                            is INamedTypeSymbol symbol
                        && InheritsFromManagerBase(symbol)
                    )
                    {
                        // 转换为纯数据，避免持有符号引用
                        return new ManagerInfo(symbol.ToDisplayString());
                    }
                    return default;
                }
            )
            .Where(static info => !string.IsNullOrEmpty(info.FullName));

        // Step 2: 从引用的程序集中提取 Manager 类和模块信息（转换为纯数据）
        var referencedDataProvider = context.CompilationProvider.Select(
            static (compilation, cancellationToken) =>
            {
                var managers = new List<ManagerInfo>();
                var modules = new List<ModuleInfo>();

                foreach (var reference in compilation.References)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (
                        compilation.GetAssemblyOrModuleSymbol(reference)
                            is IAssemblySymbol assemblySymbol
                        && assemblySymbol.Name.EndsWith("Mod", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        var assemblyName = assemblySymbol.Name;
                        var hasModuleExtensions = false;
                        var hasAddMethod = false;

                        foreach (var type in GetAllTypes(assemblySymbol.GlobalNamespace))
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (type is INamedTypeSymbol namedType)
                            {
                                // 检查是否是 Manager 类
                                if (InheritsFromManagerBase(namedType))
                                {
                                    managers.Add(new ManagerInfo(namedType.ToDisplayString()));
                                }

                                // 检查是否是 ModuleExtensions 类
                                if (
                                    type.Name == "ModuleExtensions"
                                    && type.DeclaredAccessibility == Accessibility.Public
                                    && type.IsStatic
                                )
                                {
                                    hasModuleExtensions = true;
                                    hasAddMethod = type.GetMembers()
                                        .OfType<IMethodSymbol>()
                                        .Any(m =>
                                            m.Name == $"Add{assemblyName}"
                                            && m.DeclaredAccessibility == Accessibility.Public
                                            && m.IsStatic
                                        );
                                }
                            }
                        }

                        if (hasModuleExtensions && hasAddMethod)
                        {
                            modules.Add(new ModuleInfo(assemblyName));
                        }
                    }
                }

                return new ReferencedData(
                    managers.OrderBy(m => m.FullName).ToImmutableArray(),
                    modules.OrderBy(m => m.AssemblyName).ToImmutableArray()
                );
            }
        );

        // Step 3: 获取当前程序集名称（轻量级提取）
        var assemblyNameProvider = context.CompilationProvider.Select(
            static (compilation, _) => compilation.Assembly.Name ?? "Service"
        );

        // Step 4: 合并所有数据
        var combinedData = localManagersProvider
            .Collect()
            .Combine(referencedDataProvider)
            .Combine(assemblyNameProvider);

        // Step 5: 生成源代码
        context.RegisterSourceOutput(
            combinedData,
            static (spc, data) =>
            {
                var ((localManagers, referencedData), assemblyName) = data;

                // 过滤 Share 程序集
                if (assemblyName == "Share")
                {
                    return;
                }

                // 合并所有 Manager
                var allManagers = localManagers
                    .Concat(referencedData.Managers)
                    .Distinct()
                    .OrderBy(m => m.FullName)
                    .ToImmutableArray();

                // 生成 Manager 扩展
                var managerSource = GenerateManagerExtensions(assemblyName, allManagers);
                if (!string.IsNullOrWhiteSpace(managerSource))
                {
                    spc.AddSource(
                        "__AterAutoGen__AppManagerServiceExtensions.g.cs",
                        SourceText.From(managerSource!, Encoding.UTF8)
                    );
                }

                // 生成模块扩展
                var modSource = GenerateModuleExtensions(assemblyName, referencedData.Modules);
                if (!string.IsNullOrWhiteSpace(modSource))
                {
                    spc.AddSource(
                        "__AterAutoGen__ModuleExtensions.g.cs",
                        SourceText.From(modSource!, Encoding.UTF8)
                    );
                }
            }
        );
    }

    /// <summary>
    /// 获取命名空间下的所有类型
    /// </summary>
    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            if (member is INamespaceSymbol nestedNamespace)
            {
                foreach (var type in GetAllTypes(nestedNamespace))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol namedType)
            {
                yield return namedType;
            }
        }
    }

    /// <summary>
    /// 判断节点是否包含基类列表
    /// </summary>
    private static bool HasBaseList(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl && classDecl.BaseList != null;
    }

    /// <summary>
    /// 筛选出继承自ManagerBase的类
    /// </summary>
    private static bool InheritsFromManagerBase(INamedTypeSymbol symbol)
    {
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.Name == BaseManagerName)
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    /// <summary>
    /// 生成 Manager 扩展方法
    /// </summary>
    private static string? GenerateManagerExtensions(
        string namespaceName,
        ImmutableArray<ManagerInfo> managers
    )
    {
        if (managers.IsEmpty)
        {
            return null;
        }

        var sb = new StringBuilder();
        foreach (var manager in managers)
        {
            sb.AppendLine($"        services.AddScoped<{manager.FullName}>();");
        }

        return $$"""
            // <auto-generated/>
            using Microsoft.Extensions.DependencyInjection;

            namespace {{namespaceName}}.Extension;
            public static partial class __AterAutoGen__AppManagerServiceExtensions
            {
                public static IServiceCollection AddManagers(this IServiceCollection services)
                {
            {{sb}}
                    return services;
                }
            }
            """;
    }

    /// <summary>
    /// 生成模块扩展方法
    /// </summary>
    private static string? GenerateModuleExtensions(
        string namespaceName,
        ImmutableArray<ModuleInfo> modules
    )
    {
        var usingSb = new StringBuilder();
        var registrationSb = new StringBuilder();

        foreach (var module in modules)
        {
            usingSb.AppendLine($"using {module.AssemblyName};");
            registrationSb.AppendLine($"        builder.Add{module.AssemblyName}();");
        }

        return $$"""
            // <auto-generated/>
            {{usingSb}}using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            namespace {{namespaceName}}.Extension;
            public static partial class __AterAutoGen__ModuleExtensions
            {
                public static IHostApplicationBuilder AddModules(this IHostApplicationBuilder builder)
                {
            {{registrationSb}}
                    return builder;
                }
            }
            """;
    }

    #region 数据结构（实现值相等以支持增量缓存）

    /// <summary>
    /// Manager 类信息
    /// </summary>
    private readonly struct ManagerInfo : IEquatable<ManagerInfo>
    {
        public string FullName { get; }

        public ManagerInfo(string fullName)
        {
            FullName = fullName;
        }

        public bool Equals(ManagerInfo other) => FullName == other.FullName;

        public override bool Equals(object obj) => obj is ManagerInfo other && Equals(other);

        public override int GetHashCode() => FullName?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// 模块信息
    /// </summary>
    private readonly struct ModuleInfo : IEquatable<ModuleInfo>
    {
        public string AssemblyName { get; }

        public ModuleInfo(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public bool Equals(ModuleInfo other) => AssemblyName == other.AssemblyName;

        public override bool Equals(object obj) => obj is ModuleInfo other && Equals(other);

        public override int GetHashCode() => AssemblyName?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// 引用程序集的数据
    /// </summary>
    private readonly struct ReferencedData : IEquatable<ReferencedData>
    {
        public ImmutableArray<ManagerInfo> Managers { get; }
        public ImmutableArray<ModuleInfo> Modules { get; }

        public ReferencedData(
            ImmutableArray<ManagerInfo> managers,
            ImmutableArray<ModuleInfo> modules
        )
        {
            Managers = managers;
            Modules = modules;
        }

        public bool Equals(ReferencedData other)
        {
            return Managers.SequenceEqual(other.Managers) && Modules.SequenceEqual(other.Modules);
        }

        public override bool Equals(object obj) => obj is ReferencedData other && Equals(other);

        public override int GetHashCode()
        {
            var hash = 17;
            foreach (var m in Managers)
            {
                hash = (hash * 31) + m.GetHashCode();
            }
            foreach (var m in Modules)
            {
                hash = (hash * 31) + m.GetHashCode();
            }
            return hash;
        }
    }

    #endregion
}
