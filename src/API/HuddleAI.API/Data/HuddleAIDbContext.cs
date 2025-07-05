using HuddleAI.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace HuddleAI.API.Data;

public class HuddleAIDbContext : DbContext
{
    public HuddleAIDbContext(DbContextOptions<HuddleAIDbContext> options) : base(options)
    {
    }

    public DbSet<AnalysisRequest> AnalysisRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AnalysisRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sport).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AnalysisTopic).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Overview).HasMaxLength(2000);
            entity.Property(e => e.AreasForImprovement).HasMaxLength(4000);
            entity.Property(e => e.DetailedImprovementPlan).HasMaxLength(4000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            // Performance optimization indexes based on common query patterns
            
            // Index on CreatedAt (DESC) for history queries
            // This index optimizes queries that sort by creation date, such as getting recent analysis requests
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_AnalysisRequests_CreatedAt_DESC")
                .IsDescending();

            // Composite index on Sport + CreatedAt for sport-specific queries
            // This index optimizes queries that filter by sport and sort by date (e.g., recent basketball analyses)
            entity.HasIndex(e => new { e.Sport, e.CreatedAt })
                .HasDatabaseName("IX_AnalysisRequests_Sport_CreatedAt")
                .IsDescending(false, true); // Sport ASC, CreatedAt DESC

            // Index on IsProcessed for filtering processed/unprocessed requests
            // This index optimizes queries that filter by processing status (e.g., background job queue processing)
            entity.HasIndex(e => e.IsProcessed)
                .HasDatabaseName("IX_AnalysisRequests_IsProcessed")
                .HasFilter("[IsProcessed] = 0"); // Filtered index for unprocessed requests (most common query)

            // Note: The primary key index on Id already exists and is optimal for lookups by Id
            // No additional index needed for Id as it's automatically created as a clustered index
        });
    }
}