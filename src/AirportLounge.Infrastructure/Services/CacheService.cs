using System.Collections.Concurrent;
using System.Text.Json;
using AirportLounge.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace AirportLounge.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly ConcurrentDictionary<string, byte> _trackedKeys = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        return data is null ? default : JsonSerializer.Deserialize<T>(data, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _cache.SetStringAsync(key, json, options, cancellationToken);
        _trackedKeys.TryAdd(key, 1);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
        _trackedKeys.TryRemove(key, out _);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _trackedKeys.Keys.Where(k => k.StartsWith(prefixKey)).ToList();
        foreach (var key in keysToRemove)
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _trackedKeys.TryRemove(key, out _);
        }
    }
}
