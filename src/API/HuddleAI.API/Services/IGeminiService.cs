using HuddleAI.SharedKernel.DTOs;

namespace HuddleAI.API.Services;

public interface IGeminiService
{
    Task<SportsAnalysisResponse> AnalyzeSportsPerformanceAsync(string sport, string analysisTopic, string fileBase64, string fileType);
}