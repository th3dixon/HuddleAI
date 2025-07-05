using HuddleAI.API.Data;
using HuddleAI.API.Entities;
using HuddleAI.SharedKernel.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HuddleAI.API.Repositories;

/// <summary>
/// Repository implementation for sports analysis data access operations.
/// Implements optimized queries with projections to minimize database round trips.
/// </summary>
public class AnalysisRepository : IAnalysisRepository
{
    private readonly HuddleAIDbContext _context;
    private readonly ILogger<AnalysisRepository> _logger;

    public AnalysisRepository(HuddleAIDbContext context, ILogger<AnalysisRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<AnalysisRequest> CreateAnalysisRequestAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default)
    {
        _context.AnalysisRequests.Add(analysisRequest);
        // Note: SaveChangesAsync is not called here to support transaction patterns
        // The caller should call SaveChangesAsync when appropriate
        return Task.FromResult(analysisRequest);
    }

    /// <inheritdoc />
    public Task<AnalysisRequest> UpdateAnalysisRequestAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default)
    {
        _context.AnalysisRequests.Update(analysisRequest);
        // Note: SaveChangesAsync is not called here to support transaction patterns
        // The caller should call SaveChangesAsync when appropriate
        return Task.FromResult(analysisRequest);
    }

    /// <inheritdoc />
    public async Task<List<AnalysisHistoryDto>> GetAnalysisHistoryAsync(int limit = 20, CancellationToken cancellationToken = default)
    {
        try
        {
            // Optimized query with projection to DTO to minimize data transfer
            var history = await _context.AnalysisRequests
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .Select(x => new AnalysisHistoryDto
                {
                    Id = x.Id,
                    Sport = x.Sport,
                    AnalysisTopic = x.AnalysisTopic,
                    OverallScore = x.OverallScore,
                    CreatedAt = x.CreatedAt,
                    IsProcessed = x.IsProcessed
                })
                .ToListAsync(cancellationToken);

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis history");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AnalysisDetailDto?> GetAnalysisDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Optimized query with projection to DTO
            // Includes string split for AreasForImprovement to convert from semicolon-delimited string to list
            var analysisDetail = await _context.AnalysisRequests
                .Where(a => a.Id == id)
                .Select(a => new AnalysisDetailDto
                {
                    Id = a.Id,
                    Sport = a.Sport,
                    AnalysisTopic = a.AnalysisTopic,
                    FileName = a.FileName,
                    FileType = a.FileType,
                    OverallScore = a.OverallScore,
                    Overview = a.Overview,
                    AreasForImprovement = string.IsNullOrEmpty(a.AreasForImprovement) 
                        ? new List<string>() 
                        : a.AreasForImprovement.Split(new[] { ';' }, StringSplitOptions.None).ToList(),
                    DetailedImprovementPlan = a.DetailedImprovementPlan,
                    CreatedAt = a.CreatedAt,
                    ProcessedAt = a.ProcessedAt,
                    IsProcessed = a.IsProcessed,
                    ErrorMessage = a.ErrorMessage
                })
                .FirstOrDefaultAsync(cancellationToken);
            
            return analysisDetail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis with ID {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AnalysisSummaryDto?> GetAnalysisSummaryAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Optimized query with projection to DTO
            // Includes truncation logic for Overview field (300 characters for summary view)
            var analysisSummary = await _context.AnalysisRequests
                .Where(a => a.Id == id)
                .Select(a => new AnalysisSummaryDto
                {
                    Id = a.Id,
                    Sport = a.Sport,
                    AnalysisTopic = a.AnalysisTopic,
                    OverallScore = a.OverallScore,
                    Overview = a.Overview != null && a.Overview.Length > 300 
                        ? a.Overview.Substring(0, 300) + "..." 
                        : a.Overview,
                    CreatedAt = a.CreatedAt,
                    ProcessedAt = a.ProcessedAt,
                    IsProcessed = a.IsProcessed
                })
                .FirstOrDefaultAsync(cancellationToken);
            
            return analysisSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis summary with ID {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}