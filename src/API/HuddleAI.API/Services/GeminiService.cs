using HuddleAI.SharedKernel.DTOs;
using Newtonsoft.Json;
using System.Text;

namespace HuddleAI.API.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiApiKey"] ?? throw new ArgumentNullException("GeminiApiKey");
        _logger = logger;
    }

    public async Task<SportsAnalysisResponse> AnalyzeSportsPerformanceAsync(string sport, string analysisTopic, string fileBase64, string fileType)
    {
        try
        {
            var prompt = BuildSportsAnalysisPrompt(sport, analysisTopic);
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new 
                            { 
                                inline_data = new 
                                { 
                                    mime_type = GetMimeType(fileType),
                                    data = fileBase64 
                                } 
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4,
                    topK = 32,
                    topP = 1,
                    maxOutputTokens = 4096
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}",
                content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return new SportsAnalysisResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"API Error: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            var generatedText = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text?.ToString();
            
            if (string.IsNullOrEmpty(generatedText))
            {
                return new SportsAnalysisResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "No analysis generated"
                };
            }

            return ParseAnalysisResponse(generatedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sports performance");
            return new SportsAnalysisResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private string BuildSportsAnalysisPrompt(string sport, string analysisTopic)
    {
        return $@"As a professional sports performance analyst, analyze this {sport} performance video/image focusing on {analysisTopic}.

Please provide your analysis in the following structured format:

OVERALL_SCORE: [Provide a score from 1-100 based on the performance level]

OVERVIEW: [Provide a comprehensive overview of what you observe in the video/image, including technique, form, and execution]

AREAS_FOR_IMPROVEMENT: [List 3-5 specific areas that need improvement, separated by semicolons]

DETAILED_IMPROVEMENT_PLAN: [Provide a detailed, actionable plan on how to improve the identified areas, including specific exercises, drills, or technique adjustments]

Focus specifically on {analysisTopic} aspects and provide constructive, actionable feedback that would help an athlete improve their performance.";
    }

    private SportsAnalysisResponse ParseAnalysisResponse(string analysisText)
    {
        try
        {
            var response = new SportsAnalysisResponse { IsSuccess = true };
            
            // Parse Overall Score
            var scoreMatch = System.Text.RegularExpressions.Regex.Match(analysisText, @"OVERALL_SCORE:\s*(\d+)");
            if (scoreMatch.Success && int.TryParse(scoreMatch.Groups[1].Value, out int score))
            {
                response.OverallScore = Math.Min(100, Math.Max(1, score));
            }
            else
            {
                response.OverallScore = 75; // Default score
            }

            // Parse Overview
            var overviewMatch = System.Text.RegularExpressions.Regex.Match(analysisText, @"OVERVIEW:\s*(.+?)(?=AREAS_FOR_IMPROVEMENT:|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
            response.Overview = overviewMatch.Success ? overviewMatch.Groups[1].Value.Trim() : "Analysis completed";

            // Parse Areas for Improvement
            var improvementMatch = System.Text.RegularExpressions.Regex.Match(analysisText, @"AREAS_FOR_IMPROVEMENT:\s*(.+?)(?=DETAILED_IMPROVEMENT_PLAN:|$)", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (improvementMatch.Success)
            {
                var areas = improvementMatch.Groups[1].Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
                response.AreasForImprovement = areas.Select(a => a.Trim()).ToList();
            }

            // Parse Detailed Improvement Plan
            var planMatch = System.Text.RegularExpressions.Regex.Match(analysisText, @"DETAILED_IMPROVEMENT_PLAN:\s*(.+?)$", System.Text.RegularExpressions.RegexOptions.Singleline);
            response.DetailedImprovementPlan = planMatch.Success ? planMatch.Groups[1].Value.Trim() : "Continue practicing and focus on fundamental techniques.";

            return response;
        }
        catch (Exception ex)
        {
            return new SportsAnalysisResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Error parsing analysis: {ex.Message}"
            };
        }
    }

    private string GetMimeType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "mp4" => "video/mp4",
            "avi" => "video/x-msvideo",
            "mov" => "video/quicktime",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}