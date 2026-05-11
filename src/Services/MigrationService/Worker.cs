using Microsoft.EntityFrameworkCore;
using Share.Implement;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Entity.AIAgentMod;
using Entity.KnowledgeBaseMod;
using Entity.ModelMod;
using Entity.WorkflowMod;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger
) : BackgroundService
{
    private const string FoundryLocalBaseUrlEnvVar = "AIAgent__Seed__FoundryLocalBaseUrl";
    private const string OllamaBaseUrlEnvVar = "AIAgent__Seed__OllamaBaseUrl";
    private const string DefaultFoundryLocalBaseUrl = "http://127.0.0.1:55655/v1";
    private const string DefaultOllamaBaseUrl = "http://localhost:11434/v1";
    private readonly ILogger<Worker> _logger = logger;
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(
            "Migrating database",
            ActivityKind.Client
        );
        try
        {
            using var scope = serviceProvider.CreateScope();
            _logger.LogInformation("migrations {db}", nameof(DefaultDbContext));
            var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private static async Task RunMigrationAsync<T>(T dbContext, CancellationToken cancellationToken)
        where T : DbContext
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private async Task SeedDataAsync<T>(T dbContext, CancellationToken cancellationToken)
        where T : DbContext
    {
        if (dbContext is not DefaultDbContext defaultDb)
        {
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await SeedAdminUserAsync(defaultDb, cancellationToken);
            await SeedModelProvidersAsync(defaultDb, cancellationToken);
            await SeedDemoApplicationAsync(defaultDb, cancellationToken);
            await SeedDefaultPublicAgentAsync(defaultDb, cancellationToken);
            await SeedStorageProviderAsync(defaultDb, cancellationToken);
            await SeedDefaultKnowledgeBaseAsync(defaultDb, cancellationToken);
            await SeedTranslationWorkflowAsync(defaultDb, cancellationToken);
        });
    }

    private static async Task SeedAdminUserAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.SystemUsers.AnyAsync(cancellationToken))
        {
            return;
        }

        var passwordSalt = HashCrypto.BuildSalt();
        var adminUser = new SystemUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@default.com",
            RealName = "System Administrator",
            Phone = "13800138000",
            PasswordSalt = passwordSalt,
            PasswordHash = HashCrypto.GeneratePwd("Perigon.2026", passwordSalt),
            Roles = WebConst.SuperAdmin,
            Enabled = true
        };

        db.SystemUsers.Add(adminUser);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedModelProvidersAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        var foundryLocalBaseUrl = GetSeedBaseUrl(FoundryLocalBaseUrlEnvVar, DefaultFoundryLocalBaseUrl);
        var ollamaBaseUrl = GetSeedBaseUrl(OllamaBaseUrlEnvVar, DefaultOllamaBaseUrl);

        if (await db.AIModelProviders.AnyAsync(cancellationToken))
        {
            await UpdateProviderBaseUrlAsync(db, "FoundryLocal", foundryLocalBaseUrl, cancellationToken);
            await UpdateProviderBaseUrlAsync(db, "Ollama", ollamaBaseUrl, cancellationToken);
            return;
        }

        // DeepSeek
        var deepSeek = new AIModelProvider
        {
            Name = "DeepSeek",
            Description = "DeepSeek AI - 高性能开源大语言模型提供商",
            Website = "https://www.deepseek.com",
            BaseUrl = "https://api.deepseek.com",
            LogoUrl = "https://www.deepseek.com/favicon.ico",
            ApiKey = Environment.GetEnvironmentVariable("AIAgent__Seed__DeepSeekApiKey"),
            ProviderType = ModelProviderType.OpenAiCompatible,
            Models =
            [
                new AIModelInfo { Name = "deepseek-v4-flash", DisplayName = "DeepSeek V4 Flash", Description = "DeepSeek V4 高速通用模型（1M 上下文），支持思考/非思考双模式，适合对话、工具调用和高频业务场景", ContextLength = 1048576, MaxContextTokens = 1048576, SupportsChat = true, SupportsTools = true, InputPrice = 0.14m, OutputPrice = 0.28m, IsEnabled = true },
                new AIModelInfo { Name = "deepseek-v4-pro", DisplayName = "DeepSeek V4 Pro", Description = "DeepSeek V4 旗舰深度推理模型（1M 上下文），适合复杂分析、编程与长链路推理", ContextLength = 1048576, MaxContextTokens = 1048576, SupportsChat = true, SupportsTools = true, InputPrice = 0.435m, OutputPrice = 0.87m, IsEnabled = true },
            ]
        };

        // OpenAI
        var openAI = new AIModelProvider
        {
            Name = "OpenAI",
            Description = "OpenAI - GPT 系列模型提供商",
            Website = "https://openai.com",
            BaseUrl = "https://api.openai.com/v1",
            LogoUrl = "https://openai.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "gpt-5.5", DisplayName = "GPT-5.5", Description = "OpenAI 最新旗舰推理模型（1M 上下文），具备高级推理、编码、多工具调用和 Computer Use 能力", ContextLength = 1050000, MaxContextTokens = 1050000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 5.00m, OutputPrice = 30.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-5.4", DisplayName = "GPT-5.4", Description = "OpenAI 主流旗舰模型，适合高复杂度推理、编码与智能体任务", ContextLength = 400000, MaxContextTokens = 400000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 2.50m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-5.4-mini", DisplayName = "GPT-5.4 Mini", Description = "OpenAI 当前主力小型模型，适合高频编码、工具调用与日常业务场景", ContextLength = 400000, MaxContextTokens = 400000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 0.75m, OutputPrice = 4.50m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-3-large", DisplayName = "Embedding 3 Large", Description = "OpenAI 主力文本向量模型，适合检索增强、聚类与语义搜索", ContextLength = 8191, MaxContextTokens = 8191, SupportsEmbedding = true, InputPrice = 0.13m, OutputPrice = 0m, IsEnabled = true },
            ]
        };

        // Anthropic
        var anthropic = new AIModelProvider
        {
            Name = "Anthropic",
            Description = "Anthropic - Claude 系列模型提供商",
            Website = "https://www.anthropic.com",
            BaseUrl = "https://api.anthropic.com/v1",
            LogoUrl = "https://www.anthropic.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "claude-opus-4-6", DisplayName = "Claude Opus 4.6", Description = "Anthropic 当前最强旗舰模型，适合复杂编码、长链路智能体与企业级任务", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 5.00m, OutputPrice = 25.00m, IsEnabled = true },
                new AIModelInfo { Name = "claude-sonnet-4-6", DisplayName = "Claude Sonnet 4.6", Description = "Anthropic 当前最主流平衡模型，适合生产场景中的编码、代理与专业工作流", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 3.00m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "claude-haiku-4-5", DisplayName = "Claude Haiku 4.5", Description = "Anthropic 当前最快速高性价比模型，适合高并发与低延迟场景", ContextLength = 200000, MaxContextTokens = 200000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 1.00m, OutputPrice = 5.00m, IsEnabled = true },
            ]
        };

        // Qwen (通义千问)
        var qwen = new AIModelProvider
        {
            Name = "Qwen",
            Description = "阿里云通义千问 - 国产大语言模型",
            Website = "https://dashscope.aliyun.com",
            BaseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1",
            LogoUrl = "https://img.alicdn.com/imgextra/i4/O1CN01c4CIoB1Ztqqe5wDfz_!!6000000003254-73-tps-16-16.ico",
            Models =
            [
                new AIModelInfo { Name = "qwen3-max", DisplayName = "Qwen3 Max", Description = "通义千问当前旗舰模型，适合复杂推理、长链路任务与高质量生成", ContextLength = 262144, MaxContextTokens = 262144, SupportsChat = true, SupportsTools = true, InputPrice = 2.50m, OutputPrice = 10.00m, IsEnabled = true },
                new AIModelInfo { Name = "qwen3.5-plus", DisplayName = "Qwen3.5 Plus", Description = "通义千问当前主力均衡模型，适合大多数生产场景和多模态任务", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.80m, OutputPrice = 4.80m, IsEnabled = true },
                new AIModelInfo { Name = "text-embedding-v4", DisplayName = "通义千问 Embedding V4", Description = "通义千问当前主力文本向量模型，适合语义检索与知识库场景", ContextLength = 8192, MaxContextTokens = 8192, SupportsEmbedding = true, InputPrice = 0.50m, OutputPrice = 0m, IsEnabled = true },
            ]
        };

        // Google
        var google = new AIModelProvider
        {
            Name = "Google",
            Description = "Google AI - Gemini 系列模型",
            Website = "https://ai.google.dev",
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai",
            LogoUrl = "https://www.gstatic.com/lamda/images/gemini_favicon_f069958c85030456e93de685481c559f160ea06b.png",
            Models =
            [
                new AIModelInfo { Name = "gemini-3.1-pro-preview", DisplayName = "Gemini 3.1 Pro", Description = "Google 当前最强推理与智能体模型，适合复杂多模态与长上下文任务", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 1.25m, OutputPrice = 10.00m, IsEnabled = true },
                new AIModelInfo { Name = "gemini-3-flash-preview", DisplayName = "Gemini 3 Flash", Description = "Google 当前主力高速模型，兼顾推理质量、速度与成本", ContextLength = 1000000, MaxContextTokens = 1000000, SupportsChat = true, SupportsTools = true, SupportsVision = true, InputPrice = 0.50m, OutputPrice = 3.00m, IsEnabled = true },
            ]
        };

        // Azure OpenAI
        var azure = new AIModelProvider
        {
            Name = "Azure OpenAI",
            Description = "Microsoft Azure OpenAI Service - 企业级 AI 服务",
            Website = "https://azure.microsoft.com/products/ai-services/openai-service",
            BaseUrl = "https://{your-resource}.openai.azure.com/openai/deployments/{deployment-name}",
            LogoUrl = "https://azure.microsoft.com/favicon.ico",
            Models =
            [
                new AIModelInfo { Name = "gpt-5.4", DisplayName = "Azure GPT-5.4", Description = "Azure 当前旗舰模型，适合企业级复杂推理、编码与多工具工作流", ContextLength = 1050000, MaxContextTokens = 1050000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 2.50m, OutputPrice = 15.00m, IsEnabled = true },
                new AIModelInfo { Name = "gpt-5.4-mini", DisplayName = "Azure GPT-5.4 Mini", Description = "Azure 当前主力小型模型，适合高频业务与成本敏感场景", ContextLength = 400000, MaxContextTokens = 400000, SupportsChat = true, SupportsTools = true, SupportsVision = true, SupportsResponsesApi = true, InputPrice = 0.75m, OutputPrice = 4.50m, IsEnabled = true },
            ]
        };

        db.AIModelProviders.AddRange([
            deepSeek,
            openAI,
            anthropic,
            qwen,
            google,
            azure,
            BuildFoundryLocalProvider(foundryLocalBaseUrl),
            BuildOllamaProvider(ollamaBaseUrl)
        ]);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 本地 Foundry Local provider 种子。默认 BaseUrl <c>http://127.0.0.1:55655/v1</c>（Foundry Local CLI 默认 OpenAI 兼容端口）。
    /// 若用户自定义端口可手动更新 BaseUrl。ApiKey 留空（Foundry Local 不需要认证）。
    /// 当前 Foundry Local 目录仅提供 CPU 聊天/工具模型，暂无内置 embedding 模型，仅种子 qwen3-0.6b。
    /// </summary>
    private static AIModelProvider BuildFoundryLocalProvider(string baseUrl)
    {
        return new AIModelProvider
        {
            Name = "FoundryLocal",
            Description = "Microsoft Foundry Local - 本地 OpenAI 兼容模型服务",
            Website = "https://github.com/microsoft/Foundry-Local",
            BaseUrl = baseUrl,
            LogoUrl = "https://raw.githubusercontent.com/microsoft/Foundry-Local/main/docs/logo.png",
            ApiKey = string.Empty,
            ProviderType = ModelProviderType.FoundryLocal,
            Models =
            [
                new AIModelInfo
                {
                    Name = "qwen3-0.6b-generic-cpu:4",
                    DisplayName = "Qwen3 0.6B (Local CPU)",
                    Description = "本地轻量对话/推理模型（Foundry Local catalog 别名 qwen3-0.6b），适合开发调试与离线演示",
                    ContextLength = 32768,
                    MaxContextTokens = 32768,
                    SupportsChat = true,
                    SupportsTools = true,
                    InputPrice = 0m,
                    OutputPrice = 0m,
                    IsEnabled = true,
                },
            ],
        };
    }

    /// <summary>
    /// 本地 Ollama provider 种子。默认 BaseUrl <c>http://localhost:11434/v1</c>（Aspire Ollama 集成默认宿主端口）。
    /// Ollama 暴露 OpenAI 兼容 API，因此复用 <see cref="ModelProviderType.OpenAiCompatible"/>；ApiKey 留空。
    /// 主要用于补位 Foundry Local 缺失的本地 embedding 能力（bge-m3，1024 维，中英文皆可）。
    /// </summary>
    private static AIModelProvider BuildOllamaProvider(string baseUrl)
    {
        return new AIModelProvider
        {
            Name = "Ollama",
            Description = "Ollama - 本地开源模型运行时（OpenAI 兼容）",
            Website = "https://ollama.com",
            BaseUrl = baseUrl,
            LogoUrl = "https://ollama.com/public/ollama.png",
            ApiKey = string.Empty,
            ProviderType = ModelProviderType.OpenAiCompatible,
            Models =
            [
                new AIModelInfo
                {
                    Name = "bge-m3:latest",
                    DisplayName = "BGE-M3 (Local Embedding)",
                    Description = "本地通用 embedding 模型（1024 维），多语言支持，适合知识库检索与 RAG 场景",
                    ContextLength = 8192,
                    MaxContextTokens = 8192,
                    SupportsEmbedding = true,
                    InputPrice = 0m,
                    OutputPrice = 0m,
                    IsEnabled = true,
                },
            ],
        };
    }

    private static string GetSeedBaseUrl(string envVarName, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(envVarName);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static async Task UpdateProviderBaseUrlAsync(
        DefaultDbContext db,
        string providerName,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        var provider = await db.AIModelProviders.FirstOrDefaultAsync(
            q => q.Name == providerName,
            cancellationToken);

        if (provider is null || string.Equals(provider.BaseUrl, baseUrl, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        provider.BaseUrl = baseUrl;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDemoApplicationAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        const string demoAppName = "Demo Open Platform App";

        var deepSeekFlash = await db.AIModelInfos
            .Include(q => q.Provider)
            .FirstOrDefaultAsync(q => q.Name == "deepseek-v4-flash", cancellationToken);

        if (deepSeekFlash is null)
        {
            _logger.LogWarning("Skip seeding demo application because deepseek-v4-flash model was not found.");
            return;
        }

        var application = await db.Applications
            .FirstOrDefaultAsync(q => q.Name == demoAppName, cancellationToken);

        if (application is null)
        {
            application = new Application
            {
                Name = demoAppName,
                Description = "用于第三方开放平台接入与模型调用验证的示例应用",
                IsEnabled = true,
                TenantId = deepSeekFlash.TenantId,
            };

            db.Applications.Add(application);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Seeded demo application {ApplicationName}. Generate or reset the API key through the admin application UI when needed.",
                application.Name
            );
        }

        var existingApiKeys = await db.ApiKeyAuthIndexes
            .IgnoreQueryFilters()
            .Where(q => q.ApplicationId == application.Id)
            .ToListAsync(cancellationToken);

        foreach (var apiKey in existingApiKeys)
        {
            apiKey.ApplicationName = application.Name;
            apiKey.TenantId = application.TenantId;
            apiKey.IsDeleted = false;
            apiKey.UpdatedTime = DateTimeOffset.UtcNow;
        }

        var hasPermission = await db.ApplicationModelPermissions.AnyAsync(
            q => q.ApplicationId == application.Id && q.AIModelInfoId == deepSeekFlash.Id,
            cancellationToken
        );

        if (!hasPermission)
        {
            db.ApplicationModelPermissions.Add(new ApplicationModelPermission
            {
                ApplicationId = application.Id,
                AIModelInfoId = deepSeekFlash.Id,
                IsEnabled = true,
                TenantId = application.TenantId,
            });
        }

        var existingQuotas = await db.ApplicationQuotas
            .Where(q => q.ApplicationId == application.Id)
            .Select(q => q.PeriodType)
            .ToListAsync(cancellationToken);

        if (!existingQuotas.Contains(QuotaPeriodType.Minute))
        {
            db.ApplicationQuotas.Add(new ApplicationQuota
            {
                ApplicationId = application.Id,
                PeriodType = QuotaPeriodType.Minute,
                MaxRequests = 20,
                MaxTokens = 40_000,
                WindowSeconds = 60,
                IsEnabled = true,
                TenantId = application.TenantId,
            });
        }

        if (!existingQuotas.Contains(QuotaPeriodType.Day))
        {
            db.ApplicationQuotas.Add(new ApplicationQuota
            {
                ApplicationId = application.Id,
                PeriodType = QuotaPeriodType.Day,
                MaxRequests = 1_000,
                MaxTokens = 2_000_000,
                WindowSeconds = 86_400,
                IsEnabled = true,
                TenantId = application.TenantId,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedStorageProviderAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.StorageProviders.AnyAsync(cancellationToken))
        {
            return;
        }

        var storagePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\AppData\Local\AIAgent\Storage"
            : "/var/lib/aiagent/storage";

        var localStorage = new StorageProvider
        {
            Name = "LocalStorageProvider",
            IsCloud = false,
            Path = storagePath,
            IsActive = true,
        };

        db.StorageProviders.Add(localStorage);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDefaultKnowledgeBaseAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        if (await db.RagCollections.AnyAsync(cancellationToken))
        {
            return;
        }

        var collection = new RagCollection
        {
            Name = "DefaultKnowledgeBase",
            Description = "Default knowledge base for document storage and retrieval",
            IsPublic = true,
            IsEnabled = true,
            Tags = ["default"],
        };

        db.RagCollections.Add(collection);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDefaultPublicAgentAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        const string publicAgentName = "DefaultDeepSeekAgent";

        var deepSeekFlash = await db.AIModelInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Name == "deepseek-v4-flash", cancellationToken);

        if (deepSeekFlash is null)
        {
            _logger.LogWarning("Skip seeding default public agent because deepseek-v4-flash model was not found.");
            return;
        }

        var existingAgent = await db.AIAgents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(q => q.Name == publicAgentName, cancellationToken);

        if (existingAgent is not null)
        {
            existingAgent.Description = "默认公共 Agent，用于开放平台与系统侧联调测试。";
            existingAgent.ModelId = deepSeekFlash.Name;
            existingAgent.SystemPrompt = "你是默认测试 Agent，请用简洁、可靠的方式回答用户问题。";
            existingAgent.Enable = true;
            existingAgent.IsPublic = true;
            existingAgent.IsDeleted = false;
            existingAgent.TenantId = deepSeekFlash.TenantId;
            existingAgent.UpdatedTime = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        db.AIAgents.Add(new AIAgent
        {
            Name = publicAgentName,
            Description = "默认公共 Agent，用于开放平台与系统侧联调测试。",
            ModelId = deepSeekFlash.Name,
            SystemPrompt = "你是默认测试 Agent，请用简洁、可靠的方式回答用户问题。",
            Enable = true,
            IsPublic = true,
            TenantId = deepSeekFlash.TenantId,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 种子 translator → rewriter → reviewer 工作流，用于验证 MAF Handoff/Sequential 语义。
    /// </summary>
    private async Task SeedTranslationWorkflowAsync(DefaultDbContext db, CancellationToken cancellationToken)
    {
        const string workflowName = "TranslationPipelineDemo";
        const string translatorAgentName = "DemoTranslatorAgent";
        const string rewriterAgentName = "DemoRewriterAgent";
        const string reviewerAgentName = "DemoReviewerAgent";

        var deepSeekFlash = await db.AIModelInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Name == "deepseek-v4-flash", cancellationToken);

        if (deepSeekFlash is null)
        {
            _logger.LogWarning("Skip seeding translation workflow because deepseek-v4-flash model was not found.");
            return;
        }

        if (await db.Workflows.AnyAsync(q => q.Name == workflowName, cancellationToken))
        {
            return;
        }

        var translator = await UpsertAgentAsync(db, new AIAgent
        {
            Name = translatorAgentName,
            Description = "Demo 翻译 Agent：将输入翻译成目标语言",
            ModelId = deepSeekFlash.Name,
            SystemPrompt = "你是一位专业翻译。请将用户输入忠实、地道地翻译成目标语言。仅输出翻译结果。",
            Enable = true,
            IsPublic = true,
            Capabilities = AgentCapabilities.Streaming | AgentCapabilities.Handoff,
            MemoryMode = AgentMemoryMode.Window,
            HandoffTargets = [rewriterAgentName],
            Tags = ["demo", "translation"],
            TenantId = deepSeekFlash.TenantId,
        }, cancellationToken);

        var rewriter = await UpsertAgentAsync(db, new AIAgent
        {
            Name = rewriterAgentName,
            Description = "Demo 润色 Agent：将翻译结果改写得更地道、更清晰",
            ModelId = deepSeekFlash.Name,
            SystemPrompt = "你是一位资深译后润色编辑。请在保持原意的前提下，将给定译文改写得更流畅、更符合目标语言习惯。仅输出改写后的文本。",
            Enable = true,
            IsPublic = true,
            Capabilities = AgentCapabilities.Streaming | AgentCapabilities.Handoff,
            MemoryMode = AgentMemoryMode.Window,
            HandoffTargets = [reviewerAgentName],
            Tags = ["demo", "translation"],
            TenantId = deepSeekFlash.TenantId,
        }, cancellationToken);

        var reviewer = await UpsertAgentAsync(db, new AIAgent
        {
            Name = reviewerAgentName,
            Description = "Demo 审核 Agent：对润色后的译文给出质量评估",
            ModelId = deepSeekFlash.Name,
            SystemPrompt = "你是一位翻译质量审核员。请用严格但建设性的语气对给定译文进行审核，指出问题并给出最终版本。输出格式：\n1) 评分（1-5）\n2) 问题列表\n3) 最终定稿",
            Enable = true,
            IsPublic = true,
            Capabilities = AgentCapabilities.Streaming | AgentCapabilities.StructuredOutput,
            MemoryMode = AgentMemoryMode.Window,
            Tags = ["demo", "translation"],
            TenantId = deepSeekFlash.TenantId,
        }, cancellationToken);

        var definition = new
        {
            name = workflowName,
            description = "translator → rewriter → reviewer 三段式 Demo 工作流",
            steps = new object[]
            {
                new
                {
                    id = "translate",
                    type = "agent_call",
                    agentName = translatorAgentName,
                    agentId = translator.Id,
                    input = "{{workflow.input}}",
                    next = "rewrite",
                },
                new
                {
                    id = "rewrite",
                    type = "agent_call",
                    agentName = rewriterAgentName,
                    agentId = rewriter.Id,
                    input = "{{steps.translate.output}}",
                    next = "review",
                },
                new
                {
                    id = "review",
                    type = "agent_call",
                    agentName = reviewerAgentName,
                    agentId = reviewer.Id,
                    input = "{{steps.rewrite.output}}",
                    next = null as string,
                },
            },
        };

        var workflow = new Workflow
        {
            Name = workflowName,
            Description = "Demo 翻译工作流（translator → rewriter → reviewer）",
            DefinitionJson = JsonSerializer.Serialize(definition),
            Version = 1,
            IsPublished = true,
            TenantId = deepSeekFlash.TenantId,
        };

        db.Workflows.Add(workflow);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded translation workflow {WorkflowName}", workflowName);
    }

    private static async Task<AIAgent> UpsertAgentAsync(DefaultDbContext db, AIAgent agent, CancellationToken cancellationToken)
    {
        var existing = await db.AIAgents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(q => q.Name == agent.Name, cancellationToken);

        if (existing is null)
        {
            db.AIAgents.Add(agent);
            await db.SaveChangesAsync(cancellationToken);
            return agent;
        }

        existing.Description = agent.Description;
        existing.ModelId = agent.ModelId;
        existing.SystemPrompt = agent.SystemPrompt;
        existing.Enable = agent.Enable;
        existing.IsPublic = agent.IsPublic;
        existing.IsDeleted = false;
        existing.Capabilities = agent.Capabilities;
        existing.MemoryMode = agent.MemoryMode;
        existing.HandoffTargets = agent.HandoffTargets;
        existing.Tags = agent.Tags;
        existing.TenantId = agent.TenantId;
        existing.UpdatedTime = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
