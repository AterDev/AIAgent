WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();

// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();

// AG-UI 集成（使用官方 Microsoft.Agents.AI.Hosting.AGUI.AspNetCore）
// 注意：需要配置 IChatClient 并使用 builder.AddAIAgent() 注册 Agent 后，
// 才能通过 app.MapAGUIEndpoints() 映射 AG-UI 端点
builder.Services.AddAGUIServices();

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
builder.Services.AddManagers();

// Modules, auto generate by source generator
builder.AddModules();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// TODO: 在配置好 IChatClient 并注册 Agent 后，取消注释以下代码来启用 AG-UI 端点
// app.MapAGUIEndpoints();

// 使用中间件
app.UseMiddlewareServices();

await app.RunAsync();
