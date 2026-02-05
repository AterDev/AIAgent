using FileProcessorService.Extension;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();

// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// Web中间件服务:route, openapi, jwt, default cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();

// this service's custom cors, auth, rateLimiter etc.

// add Managers, auto generate by source generator
builder.Services.AddManagers();

// add modules, auto generate by source generator
builder.AddModules();

// 添加 NATS 连接
builder.AddNatsClient("nats");

// 添加 NATS JetStream 上下文服务（官方集成方式）
builder.AddNatsJetStream();

// 添加 Qdrant 客户端（通过 Aspire 集成）
builder.AddQdrantClient("qdrant");

// 注册 RAG 处理消费者
builder.Services.AddHostedService<FileProcessorService.Workers.RagIngestionConsumer>();

// 注册后台轮询服务（用于处理 Pending/Failed 状态的文档，作为兜底机制）
builder.Services.AddHostedService<CoreMod.Services.BackgroundParsingService>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// 使用中间件
app.UseMiddlewareServices();
app.Run();
