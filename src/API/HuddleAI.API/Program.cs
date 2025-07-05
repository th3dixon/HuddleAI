using HuddleAI.API.Data;
using HuddleAI.API.Repositories;
using HuddleAI.API.Services;
using HuddleAI.API.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Add HttpContextAccessor for cache bypass functionality
builder.Services.AddHttpContextAccessor();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add Entity Framework
// Check if a SQL Server connection string is configured
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    // Use SQL Server when connection string is available (production/staging)
    builder.Services.AddDbContext<HuddleAIDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Use in-memory database for development/prototype when no SQL Server is configured
    builder.Services.AddDbContext<HuddleAIDbContext>(options =>
        options.UseInMemoryDatabase("HuddleAIDb"));
    
    // Log a warning about using in-memory database
    builder.Logging.AddConsole();
    Console.WriteLine("WARNING: Using in-memory database. Performance indexes will not be applied.");
}

// Add HTTP Client for Gemini API
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// Register services
builder.Services.AddScoped<IGeminiService, GeminiService>();

// Configure caching options
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection("CacheOptions"));

// Register memory cache
builder.Services.AddMemoryCache(options =>
{
    var cacheOptions = builder.Configuration.GetSection("CacheOptions").Get<CacheOptions>();
    if (cacheOptions?.SizeLimit > 0)
    {
        options.SizeLimit = cacheOptions.SizeLimit * 1024 * 1024; // Convert MB to bytes
    }
});

// Register cache service
builder.Services.AddSingleton<ICacheService, CacheService>();

// Register repositories
builder.Services.AddScoped<AnalysisRepository>();
builder.Services.AddScoped<IAnalysisRepository>(provider =>
{
    var innerRepository = provider.GetRequiredService<AnalysisRepository>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    var cacheOptions = provider.GetRequiredService<IOptions<CacheOptions>>();
    var logger = provider.GetRequiredService<ILogger<CachedAnalysisRepository>>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    
    return new CachedAnalysisRepository(innerRepository, cacheService, cacheOptions, logger, httpContextAccessor);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
