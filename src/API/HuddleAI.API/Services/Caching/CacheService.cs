using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HuddleAI.API.Services.Caching;

/// <summary>
/// Implementation of ICacheService using in-memory caching.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lockObject = new();

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, bool bypassCache = false) where T : class
    {
        if (bypassCache)
        {
            _logger.LogDebug("Cache bypassed for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }

        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return Task.FromResult(value);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, int absoluteExpirationMinutes) where T : class
    {
        if (value == null)
        {
            _logger.LogWarning("Attempted to cache null value for key: {Key}", key);
            return Task.CompletedTask;
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpirationMinutes))
            .RegisterPostEvictionCallback((evictedKey, evictedValue, reason, state) =>
            {
                lock (_lockObject)
                {
                    _cacheKeys.Remove(evictedKey.ToString() ?? string.Empty);
                }
                _logger.LogDebug("Cache entry evicted - Key: {Key}, Reason: {Reason}", evictedKey, reason);
            });

        _cache.Set(key, value, cacheEntryOptions);

        lock (_lockObject)
        {
            _cacheKeys.Add(key);
        }

        _logger.LogDebug("Cached value for key: {Key} with expiration: {ExpirationMinutes} minutes", key, absoluteExpirationMinutes);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);

        lock (_lockObject)
        {
            _cacheKeys.Remove(key);
        }

        _logger.LogDebug("Removed cache entry for key: {Key}", key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveByPatternAsync(string keyPattern)
    {
        var keysToRemove = new List<string>();

        lock (_lockObject)
        {
            foreach (var key in _cacheKeys)
            {
                if (key.StartsWith(keyPattern, StringComparison.OrdinalIgnoreCase))
                {
                    keysToRemove.Add(key);
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            lock (_lockObject)
            {
                _cacheKeys.Remove(key);
            }
        }

        _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, keyPattern);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
        lock (_lockObject)
        {
            foreach (var key in _cacheKeys)
            {
                _cache.Remove(key);
            }
            _cacheKeys.Clear();
        }

        _logger.LogInformation("Cleared all cache entries");
        return Task.CompletedTask;
    }
}