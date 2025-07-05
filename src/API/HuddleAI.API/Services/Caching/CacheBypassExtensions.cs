using Microsoft.AspNetCore.Http;
using System.Linq;

namespace HuddleAI.API.Services.Caching;

/// <summary>
/// Extension methods for cache bypass functionality in admin/debug scenarios.
/// </summary>
public static class CacheBypassExtensions
{
    private const string CACHE_BYPASS_HEADER = "X-Cache-Bypass";
    private const string CACHE_BYPASS_QUERY = "bypassCache";

    /// <summary>
    /// Checks if the current request should bypass the cache.
    /// Cache can be bypassed via header (X-Cache-Bypass: true) or query parameter (bypassCache=true).
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>True if cache should be bypassed, false otherwise.</returns>
    public static bool ShouldBypassCache(this HttpContext httpContext)
    {
        if (httpContext == null)
        {
            return false;
        }

        // Check header
        if (httpContext.Request.Headers.TryGetValue(CACHE_BYPASS_HEADER, out var headerValue))
        {
            if (bool.TryParse(headerValue.FirstOrDefault(), out var bypassFromHeader) && bypassFromHeader)
            {
                return true;
            }
        }

        // Check query parameter
        if (httpContext.Request.Query.TryGetValue(CACHE_BYPASS_QUERY, out var queryValue))
        {
            if (bool.TryParse(queryValue.FirstOrDefault(), out var bypassFromQuery) && bypassFromQuery)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds cache bypass information to the response headers for debugging.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="cacheHit">Whether the request resulted in a cache hit.</param>
    public static void AddCacheDebugHeaders(this HttpContext httpContext, bool cacheHit)
    {
        if (httpContext?.Response != null && !httpContext.Response.HasStarted)
        {
            httpContext.Response.Headers["X-Cache-Status"] = cacheHit ? "HIT" : "MISS";
            httpContext.Response.Headers["X-Cache-Timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
    }
}