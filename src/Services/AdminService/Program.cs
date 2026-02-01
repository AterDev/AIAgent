WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddManagers();

// 模块服务
builder.AddModules();

// 添加 NATS 连接（支持 NatsRagMessagePublisher）
builder.AddNatsClient("nats");

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// 使用中间件
app.UseMiddlewareServices();


app.Run();
