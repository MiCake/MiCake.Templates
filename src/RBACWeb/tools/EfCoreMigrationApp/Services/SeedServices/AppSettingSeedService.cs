using Microsoft.Extensions.Logging;
using RBACWeb.EFCore;

namespace EfCoreMigrationApp.Services.SeedServices;

public class AppSettingSeedService : ISeedService
{
    public int Order => 2;

    public string Name => "Initialize System Settings";

    private readonly AppDbContext _context;
    private readonly ILogger<AppSettingSeedService> _logger;

    public AppSettingSeedService(
        AppDbContext context,
        ILogger<AppSettingSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting initialization of API logging system settings...");

            // Add your seeding logic here

            return Task.CompletedTask;
        }
        catch
        {
            _logger.LogError("An error occurred while initializing API logging system settings.");
            throw;
        }
        finally
        {
            _logger.LogInformation("Completed initialization of API logging system settings.");
        }
    }

    public async Task<bool> ShouldExecuteAsync()
    {
        // here is an example condition: only seed if there are no existing system settings
        // var hasValue = await _context.AppSettings.AnyAsync(s => s.SettingGroup ==  StandardWeb.Domain.Enums.Configuration.SettingGroup.System);
        // return !hasValue;

        return true;
    }
}