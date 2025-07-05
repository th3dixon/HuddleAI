namespace HuddleAI.SharedKernel.DTOs;

/// <summary>
/// DTO for summary views that includes basic information plus a truncated overview.
/// Provides more context than history view but still optimized for performance.
/// </summary>
public class AnalysisSummaryDto
{
    /// <summary>
    /// Unique identifier for the analysis request.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The sport being analyzed (e.g., Basketball, Baseball, Soccer).
    /// </summary>
    public string Sport { get; set; } = string.Empty;
    
    /// <summary>
    /// The specific topic of analysis (e.g., Shooting Form, Swing Mechanics).
    /// </summary>
    public string AnalysisTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// Overall performance score (0-100). Null if analysis is not yet processed.
    /// </summary>
    public int? OverallScore { get; set; }
    
    /// <summary>
    /// Brief overview of the analysis results. Should be truncated to first 200-300 characters for summary views.
    /// </summary>
    public string? Overview { get; set; }
    
    /// <summary>
    /// The date and time when the analysis request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// The date and time when the analysis was completed. Null if not yet processed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Indicates whether the analysis has been completed.
    /// </summary>
    public bool IsProcessed { get; set; }
}