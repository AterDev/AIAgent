using Entity;
using Microsoft.Extensions.Logging;
using Perigon.AspNetCore.Abstraction;

namespace Services.Middleware;

/// <summary>
/// Middleware to resolve and cache tenant connection strings at request start.
/// Directly sets connection strings on ITenantContext.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IConfiguration configuration
    )
    {
        try
        {
            // 获取默认连接字符串
            var defaultConnectionString = configuration.GetConnectionString(AppConst.Default)
                ?? throw new InvalidOperationException("No default connection string configured");

            var analysisConnectionString = configuration.GetConnectionString(AppConst.Analysis)
                ?? defaultConnectionString;

            // 如果是独立租户，异步获取其专属连接字符串
            if (tenantContext.TenantType == TenantType.Independent.ToString())
            {
                var independentConnectionString = await tenantContext.GetDbConnectionStringAsync();
                var independentAnalysisConnectionString = await tenantContext.GetAnalysisConnectionStringAsync();

                // ✅ 直接设置到 ITenantContext
                tenantContext.DbConnectionString = independentConnectionString;
                tenantContext.AnalysisConnectionString = independentAnalysisConnectionString;

                _logger.LogInformation(
                    "Resolved independent tenant {TenantId} connections",
                    tenantContext.TenantId
                );
            }
            else
            {
                // ✅ 使用默认连接字符串
                tenantContext.DbConnectionString = defaultConnectionString;
                tenantContext.AnalysisConnectionString = analysisConnectionString;

                _logger.LogDebug(
                    "Using shared database for tenant {TenantId}",
                    tenantContext.TenantId
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant connection strings");
            throw;
        }

        await _next(context);
    }
}