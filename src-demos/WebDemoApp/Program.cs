using Perigon.AspNetCore.Constants;
using ServiceDefaults;

// Hosting
using Aspire.Hosting.Postgres;
using Aspire.Hosting.ApplicationModel;

namespace WebDemoApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        #region tmp

        builder.Services.AddDemoDatabase();
     
        #endregion

        #region MyRegion

        // 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
        builder.AddServiceDefaults();

        // 框架依赖服务:options, cache, dbContext
        builder.AddFrameworkServices();

        // Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
        builder.AddMiddlewareServices();

        builder
            .Services.AddAuthorizationBuilder()
            .AddPolicy(
                WebConst.User,
                policy =>
                {
                    policy.RequireRole(WebConst.User);
                }
            );

        // Managers, auto generate by source generator
       // builder.Services.AddManagers();

        // Modules, auto generate by source generator
        //  builder.AddModules();


        #endregion

        var app = builder.Build();


        app.MapDefaultEndpoints();

        // 使用中间件
        app.UseMiddlewareServices();


        app.Run();
    }
}
