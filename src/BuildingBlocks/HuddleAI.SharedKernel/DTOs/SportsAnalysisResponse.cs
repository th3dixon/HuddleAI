namespace HuddleAI.SharedKernel.DTOs;

public class SportsAnalysisResponse
{
    public int OverallScore { get; set; }
    public string Overview { get; set; } = string.Empty;
    public List<string> AreasForImprovement { get; set; } = new();
    public string DetailedImprovementPlan { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}