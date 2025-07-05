namespace HuddleAI.API.Services.Caching;

/// <summary>
/// Configuration options for the caching layer.
/// </summary>
public class CacheOptions
{
    /// <summary>
    /// Gets or sets whether caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache expiration time for analysis history in minutes.
    /// </summary>
    public int AnalysisHistoryExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the cache expiration time for individual analysis details in minutes.
    /// </summary>
    public int AnalysisDetailExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets the cache expiration time for analysis summaries in minutes.
    /// </summary>
    public int AnalysisSummaryExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets whether to log cache hit/miss information.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum cache size in MB (0 = unlimited).
    /// </summary>
    public long SizeLimit { get; set; } = 0;
}