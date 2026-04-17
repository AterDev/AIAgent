using ModelMod.Models.ModelDebugDtos;

namespace ModelMod.Services;

public class ModelDebugService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    ExtensionsAIModelClient modelClient,
    DefaultUsageMeter usageMeter,
    ILogger<ModelDebugService> logger
)
{
    public async Task<ModelDebugResponse> ChatAsync(ModelDebugRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var modelInfo = await ResolveModelAsync(dbContext, request, cancellationToken);
        await EnsureApplicationAccessAsync(dbContext, request.ApplicationId, modelInfo.Id, cancellationToken);

        var modelRequest = BuildModelRequest(modelInfo, request);
        var stopwatch = Stopwatch.StartNew();
        var response = await modelClient.ChatAsync(modelRequest, cancellationToken);
        stopwatch.Stop();

        if (!response.Success)
        {
            logger.LogWarning("Model debug request failed: {Error}", response.ErrorMessage);
        }

        var usage = usageMeter.ReadUsage(response);
        return new ModelDebugResponse
        {
            Content = response.Content ?? string.Empty,
            Model = modelInfo.Name,
            PromptTokens = usage.PromptTokens,
            CompletionTokens = usage.CompletionTokens,
            TotalTokens = usage.TotalTokens,
            FinishReason = response.Success ? "stop" : "error",
            DurationMs = (int)stopwatch.ElapsedMilliseconds,
        };
    }

    public async Task<ModelDebugStreamSession> CreateStreamSessionAsync(ModelDebugRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        var modelInfo = await ResolveModelAsync(dbContext, request, cancellationToken);
        await EnsureApplicationAccessAsync(dbContext, request.ApplicationId, modelInfo.Id, cancellationToken);

        var modelRequest = BuildModelRequest(modelInfo, request);
        var requestId = string.IsNullOrWhiteSpace(request.RequestId)
            ? Guid.NewGuid().ToString("N")
            : request.RequestId;

        return new ModelDebugStreamSession
        {
            RequestId = requestId,
            ModelName = modelInfo.Name,
            Stream = modelClient.StreamChatAsync(modelRequest, cancellationToken),
        };
    }

    private ModelRequest BuildModelRequest(AIModelInfo modelInfo, ModelDebugRequest request)
    {
        var messages = new List<ModelMessage>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new ModelMessage { Role = "system", Content = request.SystemPrompt });
        }

        messages.Add(new ModelMessage
        {
            Role = "user",
            Content = request.Prompt,
            Attachments = ModelImageInputValidator.BuildValidatedImageAttachments(request.Images),
        });

        var metadata = new Dictionary<string, string>();
        if (request.Temperature.HasValue)
        {
            metadata["temperature"] = request.Temperature.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        if (request.MaxTokens.HasValue)
        {
            metadata["max_tokens"] = request.MaxTokens.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        return new ModelRequest
        {
            Model = modelInfo.Name,
            Provider = modelInfo.Provider?.Name,
            Messages = messages,
            Metadata = metadata,
        };
    }

    private async Task<AIModelInfo> ResolveModelAsync(DefaultDbContext dbContext, ModelDebugRequest request, CancellationToken cancellationToken)
    {
        var modelInfo = await dbContext.AIModelInfos
            .AsNoTracking()
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(
                m => m.Id == request.ModelId
                    && m.TenantId == userContext.TenantId
                    && m.IsEnabled,
                cancellationToken
            );

        if (modelInfo?.Provider is null)
        {
            throw new BusinessException("Model not found or disabled");
        }

        if (!string.IsNullOrWhiteSpace(request.Provider) && !string.Equals(modelInfo.Provider.Name, request.Provider, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Model provider mismatch");
        }

        return modelInfo;
    }

    private async Task EnsureApplicationAccessAsync(DefaultDbContext dbContext, Guid? applicationId, Guid modelId, CancellationToken cancellationToken)
    {
        if (!applicationId.HasValue)
        {
            if (!userContext.IsAdmin)
            {
                throw new BusinessException("Application is required");
            }

            return;
        }

        var application = await dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == applicationId && q.TenantId == userContext.TenantId && q.IsEnabled, cancellationToken);

        if (application is null)
        {
            throw new BusinessException("Application not found");
        }

        if (userContext.IsAdmin)
        {
            return;
        }

        var allowed = await dbContext.ApplicationModelPermissions
            .AsNoTracking()
            .AnyAsync(q => q.ApplicationId == applicationId
                && q.AIModelInfoId == modelId
                && q.IsEnabled
                && q.TenantId == userContext.TenantId, cancellationToken);

        if (!allowed)
        {
            throw new BusinessException("Application is not allowed to use this model");
        }

        await EnsureQuotaAsync(dbContext, applicationId.Value, cancellationToken);
    }

    private async Task EnsureQuotaAsync(DefaultDbContext dbContext, Guid applicationId, CancellationToken cancellationToken)
    {
        var quotas = await dbContext.ApplicationQuotas
            .AsNoTracking()
            .Where(q => q.ApplicationId == applicationId && q.TenantId == userContext.TenantId && q.IsEnabled)
            .ToListAsync(cancellationToken);

        if (quotas.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var quota in quotas)
        {
            var windowSeconds = quota.WindowSeconds > 0
                ? quota.WindowSeconds
                : quota.PeriodType switch
                {
                    QuotaPeriodType.Minute => 60,
                    QuotaPeriodType.Hour => 3600,
                    QuotaPeriodType.Day => 86400,
                    QuotaPeriodType.Month => 2592000,
                    _ => 3600,
                };

            var start = now.AddSeconds(-windowSeconds);
            var query = dbContext.ModelInvocations.AsNoTracking()
                .Where(q => q.ApplicationId == applicationId
                    && q.TenantId == userContext.TenantId
                    && q.CreatedTime >= start);

            var requestCount = await query.CountAsync(cancellationToken);
            if (quota.MaxRequests > 0 && requestCount >= quota.MaxRequests)
            {
                throw new BusinessException("Application quota exceeded (requests)");
            }

            if (quota.MaxTokens > 0)
            {
                var tokens = await query.SumAsync(q => q.TotalTokens, cancellationToken);
                if (tokens >= quota.MaxTokens)
                {
                    throw new BusinessException("Application quota exceeded (tokens)");
                }
            }
        }
    }
}
