using Microsoft.Agents.AI.Hosting.AGUI.AspNetCore;

namespace ApiService.Extensions;

/// <summary>
/// AG-UI 集成扩展方法，使用官方 Microsoft.Agents.AI.Hosting.AGUI.AspNetCore 包
/// </summary>
public static class AgUiExtensions
{
    /// <summary>
    /// 添加 AG-UI 服务到应用程序
    /// </summary>
    public static IServiceCollection AddAGUIServices(this IServiceCollection services)
    {
        // 添加 HTTP 客户端和日志服务（AG-UI 所需）
        services.AddHttpClient();
        services.AddLogging();

        // 注册 AG-UI 服务
        services.AddAGUI();

        return services;
    }
}
