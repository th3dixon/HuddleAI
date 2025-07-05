namespace HuddleAI.SharedKernel.DTOs;

public class SportsAnalysisRequest
{
    public string Sport { get; set; } = string.Empty;
    public string AnalysisTopic { get; set; } = string.Empty;
    public string FileBase64 { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
}