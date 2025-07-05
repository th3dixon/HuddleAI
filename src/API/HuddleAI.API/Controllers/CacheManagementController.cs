using HuddleAI.API.Services.Caching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HuddleAI.API.Controllers;

/// <summary>
/// Controller for managing cache operations in admin/debug scenarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CacheManagementController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CacheManagementController> _logger;

    public CacheManagementController(
        ICacheService cacheService,
        IOptions<CacheOptions> cacheOptions,
        ILogger<CacheManagementController> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _cacheOptions = cacheOptions?.Value ?? throw new ArgumentNullException(nameof(cacheOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current cache configuration.
    /// </summary>
    /// <returns>The cache configuration options.</returns>
    [HttpGet("config")]
    public ActionResult<CacheOptions> GetCacheConfiguration()
    {
        _logger.LogInformation("Cache configuration requested");
        return Ok(_cacheOptions);
    }

    /// <summary>
    /// Clears all cached data.
    /// </summary>
    /// <returns>A success message.</returns>
    [HttpPost("clear")]
    public async Task<IActionResult> ClearCache()
    {
        await _cacheService.ClearAsync();
        _logger.LogInformation("Cache cleared by admin request");
        return Ok(new { message = "Cache cleared successfully" });
    }

    /// <summary>
    /// Removes cache entries by pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match cache keys for removal.</param>
    /// <returns>A success message.</returns>
    [HttpDelete("pattern/{pattern}")]
    public async Task<IActionResult> RemoveCacheByPattern(string pattern)
    {
        await _cacheService.RemoveByPatternAsync(pattern);
        _logger.LogInformation("Cache entries removed by pattern: {Pattern}", pattern);
        return Ok(new { message = $"Cache entries matching pattern '{pattern}' removed successfully" });
    }

    /// <summary>
    /// Removes a specific cache entry by key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <returns>A success message.</returns>
    [HttpDelete("key/{key}")]
    public async Task<IActionResult> RemoveCacheByKey(string key)
    {
        await _cacheService.RemoveAsync(key);
        _logger.LogInformation("Cache entry removed for key: {Key}", key);
        return Ok(new { message = $"Cache entry for key '{key}' removed successfully" });
    }
}