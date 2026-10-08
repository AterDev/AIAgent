using ModelMod.Models.ApplicationQuotaDtos;

namespace ModelMod.Managers;

/// <summary>
/// 应用配额管理
/// </summary>
public class ApplicationQuotaManager(
    TenantDbFactory dbContextFactory,
    ILogger<ApplicationQuotaManager> logger,
    IUserContext userContext,
    IDistributedCache cache
) : ManagerBase<DefaultDbContext, ApplicationQuota>(dbContextFactory, userContext, logger)
{
    private readonly TenantDbFactory _dbContextFactory = dbContextFactory;
    private readonly IDistributedCache _cache = cache;
    private const string QuotaKeyFormat = "quota:{0}:{1}:{2}";

    public async Task<PageList<ApplicationQuotaItemDto>> FilterAsync(ApplicationQuotaFilterDto filter)
    {
        Queryable = Queryable
            .Where(q => q.TenantId == _userContext.TenantId)
            .WhereNotNull(filter.ApplicationId, q => q.ApplicationId == filter.ApplicationId)
            .WhereNotNull(filter.PeriodType, q => q.PeriodType == filter.PeriodType)
            .WhereNotNull(filter.IsEnabled, q => q.IsEnabled == filter.IsEnabled);

        return await PageListAsync<ApplicationQuotaFilterDto, ApplicationQuotaItemDto>(filter);
    }

    public async Task<ApplicationQuota> AddAsync(ApplicationQuotaAddDto dto)
    {
        var entity = dto.MapTo<ApplicationQuota>();
        await InsertAsync(entity);
        return entity;
    }

    public async Task<int> EditAsync(Guid id, ApplicationQuotaUpdateDto dto)
    {
        return await UpdateAsync(id, dto);
    }

    public async Task<ApplicationQuotaDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<ApplicationQuotaDetailDto>(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    public async Task<bool> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        if (!ids.Any())
        {
            return false;
        }
        return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
    }

    public override async Task<bool> HasPermissionAsync(Guid id)
    {
        return await _dbSet.AnyAsync(q => q.Id == id && q.TenantId == _userContext.TenantId);
    }

    /// <summary>
    /// 检查是否超出配额
    /// </summary>
    public async Task<bool> CheckQuotaAsync(Guid applicationId, int estimatedTokens, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // 获取所有启用的配额规则
        var quotas = await dbContext.Set<ApplicationQuota>()
            .Where(q => q.ApplicationId == applicationId && q.IsEnabled)
            .ToListAsync(cancellationToken);

        foreach (var quota in quotas)
        {
            var windowStart = GetWindowStart(DateTime.UtcNow, quota.PeriodType);
            var cacheKey = string.Format(QuotaKeyFormat, applicationId, quota.PeriodType, windowStart);

            // 从缓存获取当前使用量
            var usageJson = await _cache.GetStringAsync(cacheKey, cancellationToken);

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
                _logger.LogWarning(
                    "Quota exceeded for application {AppId}: requests={Requests}/{MaxRequests}, tokens={Tokens}/{MaxTokens}",
                    applicationId, currentRequests, quota.MaxRequests, currentTokens + estimatedTokens, quota.MaxTokens
                );
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 消耗配额
    /// </summary>
    public async Task<QuotaConsumeResultDto> ConsumeAsync(Guid applicationId, int actualTokens, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var quota = await dbContext.Set<ApplicationQuota>()
            .FirstOrDefaultAsync(q => q.ApplicationId == applicationId && q.PeriodType == QuotaPeriodType.Day, cancellationToken);

        if (quota == null)
            throw new BusinessException("No quota configured for this application");

        var windowStart = GetWindowStart(DateTime.UtcNow, quota.PeriodType);
        var cacheKey = string.Format(QuotaKeyFormat, applicationId, quota.PeriodType, windowStart);
        var ttl = GetWindowTtl(quota.PeriodType);

        // 更新缓存
        var usageJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
        var usage = usageJson != null
            ? JsonSerializer.Deserialize<QuotaUsageCache>(usageJson)
            : new QuotaUsageCache();

        usage ??= new QuotaUsageCache();
        usage.Requests++;
        usage.Tokens += actualTokens;

        await _cache.SetStringAsync(
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

    /// <summary>
    /// 获取配额使用情况
    /// </summary>
    public async Task<QuotaUsageDto> GetUsageAsync(Guid applicationId, QuotaPeriodType periodType, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var quota = await dbContext.Set<ApplicationQuota>()
            .FirstOrDefaultAsync(q => q.ApplicationId == applicationId && q.PeriodType == periodType, cancellationToken);

        if (quota == null)
            throw new BusinessException("No quota configured");

        var windowStart = GetWindowStart(DateTime.UtcNow, periodType);
        var cacheKey = string.Format(QuotaKeyFormat, applicationId, periodType, windowStart);
        var usageJson = await _cache.GetStringAsync(cacheKey, cancellationToken);

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

    /// <summary>
    /// 重置配额
    /// </summary>
    public async Task<bool> ResetQuotaAsync(Guid applicationId, QuotaPeriodType periodType, CancellationToken cancellationToken = default)
    {
        var windowStart = GetWindowStart(DateTime.UtcNow, periodType);
        var cacheKey = string.Format(QuotaKeyFormat, applicationId, periodType, windowStart);
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("Quota reset for application {AppId}, period {PeriodType}", applicationId, periodType);
        return true;
    }

    /// <summary>
    /// 获取窗口开始时间
    /// </summary>
    private static DateTime GetWindowStart(DateTime now, QuotaPeriodType periodType) => periodType switch
    {
        QuotaPeriodType.Minute => now.AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond),
        QuotaPeriodType.Hour => now.AddMinutes(-now.Minute).AddSeconds(-now.Second).AddMilliseconds(-now.Millisecond),
        QuotaPeriodType.Day => now.Date,
        QuotaPeriodType.Month => now.AddDays(-(now.Day - 1)).Date,
        _ => throw new ArgumentOutOfRangeException(nameof(periodType))
    };

    /// <summary>
    /// 获取窗口结束时间
    /// </summary>
    private static DateTime GetWindowEnd(DateTime windowStart, QuotaPeriodType periodType) => periodType switch
    {
        QuotaPeriodType.Minute => windowStart.AddMinutes(1),
        QuotaPeriodType.Hour => windowStart.AddHours(1),
        QuotaPeriodType.Day => windowStart.AddDays(1),
        QuotaPeriodType.Month => windowStart.AddMonths(1),
        _ => throw new ArgumentOutOfRangeException(nameof(periodType))
    };

    /// <summary>
    /// 获取缓存 TTL
    /// </summary>
    private static TimeSpan GetWindowTtl(QuotaPeriodType periodType) => periodType switch
    {
        QuotaPeriodType.Minute => TimeSpan.FromMinutes(2),
        QuotaPeriodType.Hour => TimeSpan.FromHours(2),
        QuotaPeriodType.Day => TimeSpan.FromDays(2),
        QuotaPeriodType.Month => TimeSpan.FromDays(60),
        _ => TimeSpan.FromHours(1)
    };

    /// <summary>
    /// 异步更新数据库统计
    /// </summary>
    private async Task UpdateQuotaUsageAsync(Guid applicationId, QuotaPeriodType periodType, int tokens, CancellationToken cancellationToken)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

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
            _logger.LogError(ex, "Failed to update quota usage");
        }
    }

    private class QuotaUsageCache
    {
        public int Requests { get; set; }
        public long Tokens { get; set; }
    }
}
