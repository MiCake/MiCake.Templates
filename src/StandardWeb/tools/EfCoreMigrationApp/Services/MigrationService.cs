using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StandardWeb.Domain;

namespace EfCoreMigrationApp.Services;

public class MigrationService(AppDbContext context, ILogger<MigrationService> logger)
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<MigrationService> _logger = logger;

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migration...");
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during migration.");
            throw;
        }
    }
}