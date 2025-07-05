using System.ComponentModel.DataAnnotations;

namespace HuddleAI.API.Entities;

public class AnalysisRequest
{
    [Key]
    public int Id { get; set; }
    
    public string Sport { get; set; } = string.Empty;
    
    public string AnalysisTopic { get; set; } = string.Empty;
    
    public string FileName { get; set; } = string.Empty;
    
    public string FileType { get; set; } = string.Empty;
    
    public string FilePath { get; set; } = string.Empty;
    
    public int? OverallScore { get; set; }
    
    public string? Overview { get; set; }
    
    public string? AreasForImprovement { get; set; }
    
    public string? DetailedImprovementPlan { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsProcessed { get; set; }
    
    public string? ErrorMessage { get; set; }
}