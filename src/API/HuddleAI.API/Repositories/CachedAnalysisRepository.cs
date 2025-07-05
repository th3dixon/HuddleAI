using HuddleAI.API.Entities;
using HuddleAI.API.Services.Caching;
using HuddleAI.SharedKernel.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HuddleAI.API.Repositories;

/// <summary>
/// Decorator for IAnalysisRepository that adds caching capabilities.
/// Caches read operations while maintaining data consistency on writes.
/// </summary>
public class CachedAnalysisRepository : IAnalysisRepository
{
    private readonly IAnalysisRepository _innerRepository;
    private readonly ICacheService _cacheService;
    private readonly CacheOptions _cacheOptions;
    private readonly ILogger<CachedAnalysisRepository> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Cache key constants
    private const string ANALYSIS_HISTORY_KEY = "analysis-history";
    private const string ANALYSIS_DETAIL_KEY_PREFIX = "analysis-";
    private const string ANALYSIS_SUMMARY_KEY_PREFIX = "analysis-summary-";

    public CachedAnalysisRepository(
        IAnalysisRepository innerRepository,
        ICacheService cacheService,
        IOptions<CacheOptions> cacheOptions,
        ILogger<CachedAnalysisRepository> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _cacheOptions = cacheOptions?.Value ?? throw new ArgumentNullException(nameof(cacheOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public async Task<AnalysisRequest> CreateAnalysisRequestAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.CreateAnalysisRequestAsync(analysisRequest, cancellationToken);
        
        // Invalidate related caches after creation
        await InvalidateCachesAsync();
        
        _logger.LogInformation("Created analysis request {Id} and invalidated related caches", result.Id);
        return result;
    }

    /// <inheritdoc />
    public async Task<AnalysisRequest> UpdateAnalysisRequestAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.UpdateAnalysisRequestAsync(analysisRequest, cancellationToken);
        
        // Invalidate specific caches after update
        await InvalidateAnalysisCachesAsync(analysisRequest.Id);
        
        _logger.LogInformation("Updated analysis request {Id} and invalidated related caches", result.Id);
        return result;
    }

    /// <inheritdoc />
    public async Task<List<AnalysisHistoryDto>> GetAnalysisHistoryAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        var shouldBypassCache = _httpContextAccessor.HttpContext?.ShouldBypassCache() ?? false;
        
        // Check if caching is enabled and not bypassed
        if (!_cacheOptions.Enabled || shouldBypassCache)
        {
            if (shouldBypassCache)
            {
                _logger.LogDebug("Cache bypassed for analysis history request");
            }
            var result = await _innerRepository.GetAnalysisHistoryAsync(limit, cancellationToken);
            _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(false);
            return result;
        }

        // Try to get from cache first
        var cacheKey = $"{ANALYSIS_HISTORY_KEY}-{limit}";
        var cachedResult = await _cacheService.GetAsync<List<AnalysisHistoryDto>>(cacheKey);
        
        if (cachedResult != null)
        {
            _logger.LogDebug("Returning cached analysis history (limit: {Limit})", limit);
            _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(true);
            return cachedResult;
        }

        // Not in cache, get from repository
        var repositoryResult = await _innerRepository.GetAnalysisHistoryAsync(limit, cancellationToken);
        
        // Cache the result
        if (repositoryResult != null && repositoryResult.Count > 0)
        {
            await _cacheService.SetAsync(cacheKey, repositoryResult, _cacheOptions.AnalysisHistoryExpirationMinutes);
            _logger.LogDebug("Cached analysis history (limit: {Limit}) for {Minutes} minutes", limit, _cacheOptions.AnalysisHistoryExpirationMinutes);
        }

        _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(false);
        return repositoryResult;
    }

    /// <inheritdoc />
    public async Task<AnalysisDetailDto?> GetAnalysisDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var shouldBypassCache = _httpContextAccessor.HttpContext?.ShouldBypassCache() ?? false;
        
        // Check if caching is enabled and not bypassed
        if (!_cacheOptions.Enabled || shouldBypassCache)
        {
            if (shouldBypassCache)
            {
                _logger.LogDebug("Cache bypassed for analysis detail request (ID: {Id})", id);
            }
            var result = await _innerRepository.GetAnalysisDetailAsync(id, cancellationToken);
            _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(false);
            return result;
        }

        // Try to get from cache first
        var cacheKey = $"{ANALYSIS_DETAIL_KEY_PREFIX}{id}";
        var cachedResult = await _cacheService.GetAsync<AnalysisDetailDto>(cacheKey);
        
        if (cachedResult != null)
        {
            _logger.LogDebug("Returning cached analysis detail for ID: {Id}", id);
            _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(true);
            return cachedResult;
        }

        // Not in cache, get from repository
        var repositoryResult = await _innerRepository.GetAnalysisDetailAsync(id, cancellationToken);
        
        // Cache the result
        if (repositoryResult != null)
        {
            await _cacheService.SetAsync(cacheKey, repositoryResult, _cacheOptions.AnalysisDetailExpirationMinutes);
            _logger.LogDebug("Cached analysis detail for ID: {Id} for {Minutes} minutes", id, _cacheOptions.AnalysisDetailExpirationMinutes);
        }

        _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(false);
        return repositoryResult;
    }

    /// <inheritdoc />
    public async Task<AnalysisSummaryDto?> GetAnalysisSummaryAsync(int id, CancellationToken cancellationToken = default)
    {
        var shouldBypassCache = _httpContextAccessor.HttpContext?.ShouldBypassCache() ?? false;
        
        // Check if caching is enabled and not bypassed
        if (!_cacheOptions.Enabled || shouldBypassCache)
        {
            if (shouldBypassCache)
            {
                _logger.LogDebug("Cache bypassed for analysis summary request (ID: {Id})", id);
            }
            var result = await _innerRepository.GetAnalysisSummaryAsync(id, cancellationToken);
            _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(false);
            return result;
        }

        // Try to get from cache first
        var cacheKey = $"{ANALYSIS_SUMMARY_KEY_PREFIX}{id}";
        var cachedResult = await _cacheService.GetAsync<AnalysisSummaryDto>(cacheKey);
        
        if (cachedResult != null)
        {
            _logger.LogDebug("Returning cached analysis summary for ID: {Id}", id);
            _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(true);
            return cachedResult;
        }

        // Not in cache, get from repository
        var repositoryResult = await _innerRepository.GetAnalysisSummaryAsync(id, cancellationToken);
        
        // Cache the result
        if (repositoryResult != null)
        {
            await _cacheService.SetAsync(cacheKey, repositoryResult, _cacheOptions.AnalysisSummaryExpirationMinutes);
            _logger.LogDebug("Cached analysis summary for ID: {Id} for {Minutes} minutes", id, _cacheOptions.AnalysisSummaryExpirationMinutes);
        }

        _httpContextAccessor.HttpContext?.AddCacheDebugHeaders(false);
        return repositoryResult;
    }

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _innerRepository.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _innerRepository.SaveChangesAsync(cancellationToken);
        
        // Invalidate caches after saving changes
        if (result > 0)
        {
            await InvalidateCachesAsync();
            _logger.LogDebug("Invalidated caches after saving {Count} changes", result);
        }

        return result;
    }

    /// <summary>
    /// Invalidates all analysis-related caches.
    /// </summary>
    private async Task InvalidateCachesAsync()
    {
        await _cacheService.RemoveByPatternAsync(ANALYSIS_HISTORY_KEY);
        await _cacheService.RemoveByPatternAsync(ANALYSIS_DETAIL_KEY_PREFIX);
        await _cacheService.RemoveByPatternAsync(ANALYSIS_SUMMARY_KEY_PREFIX);
    }

    /// <summary>
    /// Invalidates caches for a specific analysis ID.
    /// </summary>
    private async Task InvalidateAnalysisCachesAsync(int analysisId)
    {
        // Invalidate history cache (as it might contain this analysis)
        await _cacheService.RemoveByPatternAsync(ANALYSIS_HISTORY_KEY);
        
        // Invalidate specific analysis caches
        await _cacheService.RemoveAsync($"{ANALYSIS_DETAIL_KEY_PREFIX}{analysisId}");
        await _cacheService.RemoveAsync($"{ANALYSIS_SUMMARY_KEY_PREFIX}{analysisId}");
    }
}