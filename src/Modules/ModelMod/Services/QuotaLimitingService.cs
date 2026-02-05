using ModelMod.Models.ApplicationQuotaDtos;

namespace ModelMod.Services;

/// <summary>
/// 配额限流服务
/// </summary>
public class QuotaLimitingService(
    TenantDbFactory dbContextFactory,
    IDistributedCache cache,
    ILogger<QuotaLimitingService> logger
)
{
    private const string QuotaKeyFormat = "quota:{0}:{1}:{2}";

    public async Task<bool> CheckQuotaAsync(Guid applicationId, int estimatedTokens, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        // 获取所有启用的配额规则
        var quotas = await dbContext.Set<ApplicationQuota>()
            .Where(q => q.ApplicationId == applicationId && q.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var quota in quotas)
        {
            var windowStart = GetWindowStart(DateTime.UtcNow, quota.PeriodType);
            var cacheKey = string.Format(QuotaKeyFormat, applicationId, quota.PeriodType, windowStart);

            // 从缓存获取当前使用量
            var usageJson = await cache.GetStringAsync(cacheKey, cancellationToken);

            int currentRequests = 0;
            long currentTokens = 0;

            if (usageJson != null)
            {
                var usage = JsonSerializer.Deserialize<QuotaUsageCache>(usageJson);
                if (usage != null)
                {
                    currentRequests = usage.Requests;
                    currentTokens = usage.Tokens;
                }
            }

            // 检查是否超配额
            if (currentRequests >= quota.MaxRequests || currentTokens + estimatedTokens > quota.MaxTokens)
            {
                logger.LogWarning(
                    "Quota exceeded for application {AppId}: requests={Requests}/{MaxRequests}, tokens={Tokens}/{MaxTokens}",
                    applicationId, currentRequests, quota.MaxRequests, currentTokens + estimatedTokens, quota.MaxTokens
                );
                return false;
            }
        }

        return true;
    }

    public async Task<QuotaConsumeResultDto> ConsumeAsync(Guid applicationId, int actualTokens, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var quota = await dbContext.Set<ApplicationQuota>()
            .FirstOrDefaultAsync(q => q.ApplicationId == applicationId && q.PeriodType == QuotaPeriodType.Day, cancellationToken);

        if (quota == null)
        {
            throw new BusinessException("No quota configured for this application");
        }

        var windowStart = GetWindowStart(DateTime.UtcNow, quota.PeriodType);
        var cacheKey = string.Format(QuotaKeyFormat, applicationId, quota.PeriodType, windowStart);
        var ttl = GetWindowTtl(quota.PeriodType);

        // 更新缓存
        var usageJson = await cache.GetStringAsync(cacheKey, cancellationToken);
        var usage = usageJson != null
            ? JsonSerializer.Deserialize<QuotaUsageCache>(usageJson)
            : new QuotaUsageCache();

        usage ??= new QuotaUsageCache();
        usage.Requests++;
        usage.Tokens += actualTokens;

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(usage),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            cancellationToken
        );

        // 异步更新数据库统计
        _ = UpdateQuotaUsageAsync(applicationId, quota.PeriodType, actualTokens, cancellationToken);

        var windowEnd = GetWindowEnd(windowStart, quota.PeriodType);
        return new QuotaConsumeResultDto
        {
            Success = true,
            RemainingRequests = Math.Max(0, quota.MaxRequests - usage.Requests),
            RemainingTokens = Math.Max(0, quota.MaxTokens - (int)usage.Tokens),
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            UsagePercentage = quota.MaxTokens > 0 ? (usage.Tokens * 100.0) / quota.MaxTokens : 0
        };
    }

    public async Task<QuotaUsageDto> GetUsageAsync(Guid applicationId, QuotaPeriodType periodType, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var quota = await dbContext.Set<ApplicationQuota>()
            .FirstOrDefaultAsync(q => q.ApplicationId == applicationId && q.PeriodType == periodType, cancellationToken);

        if (quota == null)
        {
            throw new BusinessException("No quota configured");
        }

        var windowStart = GetWindowStart(DateTime.UtcNow, periodType);
        var cacheKey = string.Format(QuotaKeyFormat, applicationId, periodType, windowStart);
        var usageJson = await cache.GetStringAsync(cacheKey, cancellationToken);

        var usage = usageJson != null
            ? JsonSerializer.Deserialize<QuotaUsageCache>(usageJson)
            : new QuotaUsageCache();

        usage ??= new QuotaUsageCache();

        var windowEnd = GetWindowEnd(windowStart, periodType);
        return new QuotaUsageDto
        {
            ApplicationId = applicationId,
            PeriodType = periodType,
            MaxRequests = quota.MaxRequests,
            MaxTokens = quota.MaxTokens,
            CurrentRequests = usage.Requests,
            CurrentTokens = usage.Tokens,
            WindowStart = windowStart,
            WindowEnd = windowEnd
        };
    }

    public async Task<bool> ResetQuotaAsync(Guid applicationId, QuotaPeriodType periodType, CancellationToken cancellationToken = default)
    {
        var windowStart = GetWindowStart(DateTime.UtcNow, periodType);
        var cacheKey = string.Format(QuotaKeyFormat, applicationId, periodType, windowStart);
        await cache.RemoveAsync(cacheKey, cancellationToken);

        logger.LogInformation("Quota reset for application {AppId}, period {PeriodType}", applicationId, periodType);
        return true;
    }

    private static DateTime GetWindowStart(DateTime now, QuotaPeriodType periodType) => periodType switch
    {
        QuotaPeriodType.Minute => now.AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond),
        QuotaPeriodType.Hour => now.AddMinutes(-now.Minute).AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond),
        QuotaPeriodType.Day => now.Date,
        QuotaPeriodType.Month => now.AddDays(-(now.Day - 1)).Date,
        _ => throw new ArgumentOutOfRangeException(nameof(periodType))
    };

    private static DateTime GetWindowEnd(DateTime windowStart, QuotaPeriodType periodType) => periodType switch
    {
        QuotaPeriodType.Minute => windowStart.AddMinutes(1),
        QuotaPeriodType.Hour => windowStart.AddHours(1),
        QuotaPeriodType.Day => windowStart.AddDays(1),
        QuotaPeriodType.Month => windowStart.AddMonths(1),
        _ => throw new ArgumentOutOfRangeException(nameof(periodType))
    };

    private static TimeSpan GetWindowTtl(QuotaPeriodType periodType) => periodType switch
    {
        QuotaPeriodType.Minute => TimeSpan.FromMinutes(2),
        QuotaPeriodType.Hour => TimeSpan.FromHours(2),
        QuotaPeriodType.Day => TimeSpan.FromDays(2),
        QuotaPeriodType.Month => TimeSpan.FromDays(60),
        _ => TimeSpan.FromHours(1)
    };

    private async Task UpdateQuotaUsageAsync(Guid applicationId, QuotaPeriodType periodType, int tokens, CancellationToken cancellationToken)
    {
        try
        {
            await using var dbContext = await dbContextFactory.CreateDbContextAsync();

            var windowStart = GetWindowStart(DateTime.UtcNow, periodType);
            var windowEnd = GetWindowEnd(windowStart, periodType);

            var usage = await dbContext.Set<QuotaUsage>()
                .FirstOrDefaultAsync(
                    q => q.ApplicationId == applicationId
                      && q.PeriodType == periodType
                      && q.WindowStart == windowStart,
                    cancellationToken
                );

            if (usage != null)
            {
                usage.RequestCount++;
                usage.TokensUsed += tokens;
                usage.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                usage = new QuotaUsage
                {
                    ApplicationId = applicationId,
                    PeriodType = periodType,
                    WindowStart = windowStart,
                    WindowEnd = windowEnd,
                    RequestCount = 1,
                    TokensUsed = tokens
                };
                await dbContext.Set<QuotaUsage>().AddAsync(usage, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update quota usage");
        }
    }

    private class QuotaUsageCache
    {
        public int Requests { get; set; }
        public long Tokens { get; set; }
    }
}
