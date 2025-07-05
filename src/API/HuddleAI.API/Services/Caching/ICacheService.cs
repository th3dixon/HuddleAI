using System.Threading.Tasks;

namespace HuddleAI.API.Services.Caching;

/// <summary>
/// Provides a generic caching interface for storing and retrieving cached data.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="bypassCache">If true, bypasses the cache and returns null.</param>
    /// <returns>The cached value if found, otherwise null.</returns>
    Task<T?> GetAsync<T>(string key, bool bypassCache = false) where T : class;

    /// <summary>
    /// Sets a value in the cache with specified expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="absoluteExpirationMinutes">The absolute expiration time in minutes.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync<T>(string key, T value, int absoluteExpirationMinutes) where T : class;

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes multiple values from the cache based on a key pattern.
    /// </summary>
    /// <param name="keyPattern">The pattern to match cache keys for removal.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveByPatternAsync(string keyPattern);

    /// <summary>
    /// Clears all cached data.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync();
}