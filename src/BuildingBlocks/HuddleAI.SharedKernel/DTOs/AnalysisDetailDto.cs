namespace HuddleAI.SharedKernel.DTOs;

/// <summary>
/// Comprehensive DTO for detailed analysis views containing all user-relevant fields.
/// Excludes sensitive internal fields like file paths for security.
/// </summary>
public class AnalysisDetailDto
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
    /// Original filename of the uploaded video or image.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// File type/extension (e.g., mp4, jpg, png).
    /// </summary>
    public string FileType { get; set; } = string.Empty;
    
    /// <summary>
    /// Overall performance score (0-100). Null if analysis is not yet processed.
    /// </summary>
    public int? OverallScore { get; set; }
    
    /// <summary>
    /// Complete overview of the analysis results.
    /// </summary>
    public string? Overview { get; set; }
    
    /// <summary>
    /// List of areas identified for improvement. 
    /// Deserialized from JSON string stored in the entity.
    /// </summary>
    public List<string> AreasForImprovement { get; set; } = new();
    
    /// <summary>
    /// Detailed plan for improving performance based on the analysis.
    /// </summary>
    public string? DetailedImprovementPlan { get; set; }
    
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
    
    /// <summary>
    /// Error message if the analysis failed. Null for successful analyses.
    /// </summary>
    public string? ErrorMessage { get; set; }
}