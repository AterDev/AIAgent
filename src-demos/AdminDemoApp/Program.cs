using Perigon.AspNetCore.Constants;
using ServiceDefaults;

using AdminDemoApp.Extension;

namespace AdminDemoApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region MyRegion
        // 前提：appsettings配置：Components

        // 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
        builder.AddServiceDefaults();

        // 框架依赖服务:options, cache, dbContext
        builder.AddFrameworkServices();

        // Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
        builder.AddMiddlewareServices();

        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                WebConst.AdminUser,
                policy =>
                {
                    policy.RequireRole(WebConst.AdminUser, WebConst.SuperAdmin);
                }
            )
            .AddPolicy(
                WebConst.SuperAdmin,
                policy =>
                {
                    policy.RequireRole(WebConst.SuperAdmin);
                }
            );

        // 业务Managers
        // 注意引用项目(Perigon.AspNetCore.SourceGeneration)和(xxxMod)和命名空间(xxx.Extension)，并在项目文件配置 OutputItemType="Analyzer" ReferenceOutputAssembly="false"
        builder.Services.AddManagers();

        // 模块服务
        builder.AddModules();


        #endregion

        // Add services to the container.
        // tmp
        //builder.Services.AddHealthChecks();
        //builder.Services.AddControllersWithViews();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // 使用中间件
        app.UseMiddlewareServices();


        app.Run();
    }
}
