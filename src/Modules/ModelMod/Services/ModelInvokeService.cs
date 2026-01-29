namespace ModelMod.Services;

/// <summary>
/// 模型调用与审计
/// </summary>
public class ModelInvokeService(
    TenantDbFactory dbContextFactory,
    IUserContext userContext,
    IModelClient modelClient,
    IUsageMeter usageMeter,
    ILogger<ModelInvokeService> logger
) : IModelInvokeService
{
    public Task<ModelResponse> ChatAsync(Guid applicationId, ModelRequest request, CancellationToken cancellationToken = default)
    {
        return InvokeAsync(applicationId, request, modelClient.ChatAsync, cancellationToken);
    }

    public Task<ModelResponse> EmbeddingAsync(Guid applicationId, ModelRequest request, CancellationToken cancellationToken = default)
    {
        return InvokeAsync(applicationId, request, modelClient.EmbeddingAsync, cancellationToken);
    }

    private async Task<ModelResponse> InvokeAsync(
        Guid applicationId,
        ModelRequest request,
        Func<ModelRequest, CancellationToken, Task<ModelResponse>> action,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var application = await dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == applicationId && q.TenantId == userContext.TenantId && q.IsEnabled, cancellationToken);

        if (application is null)
        {
            throw new BusinessException("Application not found");
        }

        var modelQuery = from modelInfo in dbContext.AIModelInfos.AsNoTracking()
                         join provider in dbContext.ModelProviders.AsNoTracking()
                             on modelInfo.ProviderId equals provider.Id
                         where modelInfo.TenantId == userContext.TenantId
                             && provider.TenantId == userContext.TenantId
                             && modelInfo.IsEnabled
                             && provider.IsEnabled
                             && modelInfo.Name == request.Model
                         select new { modelInfo, provider };

        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            modelQuery = modelQuery.Where(q => q.provider.Name == request.Provider);
        }

        var model = await modelQuery.FirstOrDefaultAsync(cancellationToken);
        if (model is null)
        {
            throw new BusinessException("Model not found or disabled");
        }

        var allowed = await dbContext.ApplicationModelPermissions
            .AsNoTracking()
            .AnyAsync(q => q.ApplicationId == applicationId
                && q.AIModelInfoId == model.modelInfo.Id
                && q.IsEnabled
                && q.TenantId == userContext.TenantId, cancellationToken);

        if (!allowed)
        {
            throw new BusinessException("Application is not allowed to use this model");
        }

        await EnsureQuotaAsync(dbContext, applicationId, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var response = await action(request, cancellationToken);
        stopwatch.Stop();

        var usage = usageMeter.ReadUsage(response);
        var invocation = new ModelInvocation
        {
            ApplicationId = applicationId,
            AIModelInfoId = model.modelInfo.Id,
            Scene = request.Scene ?? string.Empty,
            PromptTokens = usage.PromptTokens,
            CompletionTokens = usage.CompletionTokens,
            TotalTokens = usage.TotalTokens,
            DurationMs = (int)stopwatch.ElapsedMilliseconds,
            Status = response.Success ? InvocationStatus.Success : InvocationStatus.Failed,
            ErrorMessage = response.ErrorMessage,
            TenantId = userContext.TenantId,
        };

        dbContext.ModelInvocations.Add(invocation);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (!response.Success)
        {
            logger.LogWarning("Model invocation failed: {Error}", response.ErrorMessage);
        }

        return response;
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
