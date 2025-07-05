using HuddleAI.API.Entities;
using HuddleAI.API.Repositories;
using HuddleAI.API.Services;
using HuddleAI.SharedKernel.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HuddleAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SportsAnalysisController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IAnalysisRepository _repository;
    private readonly ILogger<SportsAnalysisController> _logger;

    public SportsAnalysisController(IGeminiService geminiService, IAnalysisRepository repository, ILogger<SportsAnalysisController> logger)
    {
        _geminiService = geminiService;
        _repository = repository;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<SportsAnalysisResponse>> AnalyzePerformance([FromBody] SportsAnalysisRequest request, CancellationToken cancellationToken)
    {
        // Use a transaction to ensure data consistency
        using var transaction = await _repository.BeginTransactionAsync(cancellationToken);
        
        try
        {
            if (string.IsNullOrEmpty(request.Sport) || string.IsNullOrEmpty(request.AnalysisTopic))
            {
                return BadRequest("Sport and AnalysisTopic are required.");
            }

            // Validate media source
            if (request.MediaType == "youtube")
            {
                if (string.IsNullOrEmpty(request.YoutubeUrl))
                {
                    return BadRequest("YouTube URL is required when MediaType is 'youtube'.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(request.FileBase64))
                {
                    return BadRequest("FileBase64 is required when MediaType is 'file'.");
                }
            }

            // Create the analysis request entity
            var analysisRequest = new AnalysisRequest
            {
                Sport = request.Sport,
                AnalysisTopic = request.AnalysisTopic,
                MediaType = request.MediaType,
                FileName = request.MediaType == "youtube" ? "YouTube Video" : request.FileName,
                FileType = request.MediaType == "youtube" ? "video" : request.FileType,
                FilePath = request.MediaType == "youtube" ? request.YoutubeUrl : $"temp/{Guid.NewGuid()}_{request.FileName}",
                YoutubeUrl = request.MediaType == "youtube" ? request.YoutubeUrl : null,
                CreatedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            // Add to repository but don't save yet - this allows us to get the ID after SaveChanges
            await _repository.CreateAnalysisRequestAsync(analysisRequest, cancellationToken);

            // Call Gemini API for analysis before saving to database
            // This optimizes database calls by avoiding the need to save twice
            SportsAnalysisResponse response;
            try
            {
                if (request.MediaType == "youtube")
                {
                    response = await _geminiService.AnalyzeSportsPerformanceFromYouTubeAsync(
                        request.Sport, 
                        request.AnalysisTopic, 
                        request.YoutubeUrl);
                }
                else
                {
                    response = await _geminiService.AnalyzeSportsPerformanceAsync(
                        request.Sport, 
                        request.AnalysisTopic, 
                        request.FileBase64, 
                        request.FileType);
                }
                
                // Update the entity with successful analysis results
                analysisRequest.OverallScore = response.OverallScore;
                analysisRequest.Overview = response.Overview;
                analysisRequest.AreasForImprovement = string.Join(";", response.AreasForImprovement);
                analysisRequest.DetailedImprovementPlan = response.DetailedImprovementPlan;
                analysisRequest.ProcessedAt = DateTime.UtcNow;
                analysisRequest.IsProcessed = true;
                analysisRequest.ErrorMessage = response.ErrorMessage;
            }
            catch (Exception geminiEx)
            {
                // If Gemini API fails, still save the request with error details
                _logger.LogError(geminiEx, "Gemini API error for analysis request");
                
                analysisRequest.ProcessedAt = DateTime.UtcNow;
                analysisRequest.IsProcessed = true;
                analysisRequest.ErrorMessage = $"Gemini API error: {geminiEx.Message}";
                
                // Create error response
                response = new SportsAnalysisResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Analysis service temporarily unavailable. Request has been logged."
                };
            }

            // Single SaveChanges call - optimized from 2 calls to 1
            // This saves both the initial request and the analysis results in one database round trip
            await _repository.SaveChangesAsync(cancellationToken);
            
            // Commit the transaction
            await transaction.CommitAsync();

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Rollback transaction on any error
            await transaction.RollbackAsync();
            
            _logger.LogError(ex, "Error processing sports analysis request");
            return StatusCode(500, new SportsAnalysisResponse
            {
                IsSuccess = false,
                ErrorMessage = "Internal server error occurred while processing the analysis."
            });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<AnalysisHistoryDto>>> GetAnalysisHistory(CancellationToken cancellationToken)
    {
        try
        {
            var history = await _repository.GetAnalysisHistoryAsync(20, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis history");
            return StatusCode(500, "Error retrieving analysis history");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AnalysisDetailDto>> GetAnalysisById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var analysisDetail = await _repository.GetAnalysisDetailAsync(id, cancellationToken);
            
            if (analysisDetail == null)
            {
                return NotFound(new { message = $"Analysis with ID {id} not found." });
            }

            return Ok(analysisDetail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis with ID {Id}", id);
            return StatusCode(500, new { message = "Error retrieving analysis", error = ex.Message });
        }
    }

    [HttpGet("{id}/summary")]
    public async Task<ActionResult<AnalysisSummaryDto>> GetAnalysisSummaryById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var analysisSummary = await _repository.GetAnalysisSummaryAsync(id, cancellationToken);
            
            if (analysisSummary == null)
            {
                return NotFound(new { message = $"Analysis with ID {id} not found." });
            }

            return Ok(analysisSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis summary with ID {Id}", id);
            return StatusCode(500, new { message = "Error retrieving analysis summary", error = ex.Message });
        }
    }
}