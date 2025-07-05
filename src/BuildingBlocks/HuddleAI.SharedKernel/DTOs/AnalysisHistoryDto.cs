namespace HuddleAI.SharedKernel.DTOs;

/// <summary>
/// Lightweight DTO for displaying analysis history with minimal fields to reduce network payload.
/// Used for listing views where only essential information is needed.
/// </summary>
public class AnalysisHistoryDto
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
    /// The date and time when the analysis request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Indicates whether the analysis has been completed.
    /// </summary>
    public bool IsProcessed { get; set; }
}