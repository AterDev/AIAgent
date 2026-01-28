using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Share.Services;

/// <summary>
/// 缓存服务，用于优化热数据查询性能
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}

/// <summary>
/// Redis 缓存服务实现
/// </summary>
public class RedisCacheService(
    IDistributedCache distributedCache,
    ILogger<RedisCacheService> logger
) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cached = await distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrWhiteSpace(cached))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            await distributedCache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove cache for key: {Key}", key);
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Redis pattern-based deletion requires StackExchange.Redis
        // This is a simplified implementation
        logger.LogWarning("Pattern-based cache removal not implemented. Use specific keys instead.");
        return Task.CompletedTask;
    }
}
