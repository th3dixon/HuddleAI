using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HuddleAI.API.Data;

/// <summary>
/// Design-time factory for HuddleAIDbContext.
/// This is used by Entity Framework Core tools for migrations when the application is not running.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HuddleAIDbContext>
{
    public HuddleAIDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HuddleAIDbContext>();
        
        // Use SQL Server for design-time operations (migrations)
        // This connection string is only used for generating migrations, not runtime
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=HuddleAIDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new HuddleAIDbContext(optionsBuilder.Options);
    }
}