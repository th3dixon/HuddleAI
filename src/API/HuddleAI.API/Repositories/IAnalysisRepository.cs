using HuddleAI.API.Entities;
using HuddleAI.SharedKernel.DTOs;

namespace HuddleAI.API.Repositories;

/// <summary>
/// Repository interface for sports analysis data access operations.
/// Provides optimized queries with projections for performance.
/// </summary>
public interface IAnalysisRepository
{
    /// <summary>
    /// Creates a new analysis request in the database.
    /// </summary>
    /// <param name="analysisRequest">The analysis request entity to create.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created analysis request with generated ID.</returns>
    Task<AnalysisRequest> CreateAnalysisRequestAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing analysis request with processing results.
    /// </summary>
    /// <param name="analysisRequest">The analysis request entity to update.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated analysis request.</returns>
    Task<AnalysisRequest> UpdateAnalysisRequestAsync(AnalysisRequest analysisRequest, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves analysis history with optimized projection to DTO.
    /// </summary>
    /// <param name="limit">Maximum number of records to return (default: 20).</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>List of analysis history DTOs ordered by most recent.</returns>
    Task<List<AnalysisHistoryDto>> GetAnalysisHistoryAsync(int limit = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets detailed analysis information by ID with optimized projection to DTO.
    /// </summary>
    /// <param name="id">The analysis request ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Analysis detail DTO or null if not found.</returns>
    Task<AnalysisDetailDto?> GetAnalysisDetailAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets analysis summary by ID with optimized projection to DTO.
    /// </summary>
    /// <param name="id">The analysis request ID.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Analysis summary DTO or null if not found.</returns>
    Task<AnalysisSummaryDto?> GetAnalysisSummaryAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The database transaction.</returns>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}